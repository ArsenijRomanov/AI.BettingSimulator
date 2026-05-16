from __future__ import annotations

from typing import Final

import pandas as pd


BASELINE_FEATURE_COLUMNS: Final[list[str]] = [
    "team",
    "opponent",
    "is_home",
    "league",
    "season",
]

TARGET_COLUMN: Final[str] = "goals"


_REQUIRED_MATCH_COLUMNS: Final[list[str]] = [
    "understat_id",
    "date",
    "league",
    "season",
    "home_team",
    "away_team",
    "home_goals",
    "away_goals",
]


_OPTIONAL_MATCH_COLUMNS: Final[list[str]] = [
    "home_xg",
    "away_xg",
]


def validate_matches_dataframe(matches: pd.DataFrame) -> None:
    """
    Проверяет, что исходный dataframe матчей содержит нужные колонки.
    """

    missing_columns = sorted(set(_REQUIRED_MATCH_COLUMNS) - set(matches.columns))

    if missing_columns:
        raise ValueError(f"Missing required columns: {missing_columns}")


def make_team_match_rows(
    matches: pd.DataFrame,
    include_extra_columns: bool = True,
) -> pd.DataFrame:
    """
    Превращает таблицу матчей в таблицу формата "одна команда в одном матче".
    Возвращает Dataframe с колонками для обучения Poisson-регрессии.
    """

    validate_matches_dataframe(matches)

    df = matches.copy()
    df["date"] = pd.to_datetime(df["date"])

    has_xg = {"home_xg", "away_xg"}.issubset(df.columns)

    home_columns = [
        "understat_id",
        "date",
        "league",
        "season",
        "home_team",
        "away_team",
        "home_goals",
    ]

    if include_extra_columns:
        home_columns.append("away_goals")

        if has_xg:
            home_columns.extend(["home_xg", "away_xg"])

    home_rows = df[home_columns].copy()

    home_rename_map = {
        "home_team": "team",
        "away_team": "opponent",
        "home_goals": "goals",
    }

    if include_extra_columns:
        home_rename_map["away_goals"] = "goals_against"

        if has_xg:
            home_rename_map["home_xg"] = "xg_for"
            home_rename_map["away_xg"] = "xg_against"

    home_rows = home_rows.rename(columns=home_rename_map)
    home_rows["is_home"] = 1

    away_columns = [
        "understat_id",
        "date",
        "league",
        "season",
        "away_team",
        "home_team",
        "away_goals",
    ]

    if include_extra_columns:
        away_columns.append("home_goals")

        if has_xg:
            away_columns.extend(["away_xg", "home_xg"])

    away_rows = df[away_columns].copy()

    away_rename_map = {
        "away_team": "team",
        "home_team": "opponent",
        "away_goals": "goals",
    }

    if include_extra_columns:
        away_rename_map["home_goals"] = "goals_against"

        if has_xg:
            away_rename_map["away_xg"] = "xg_for"
            away_rename_map["home_xg"] = "xg_against"

    away_rows = away_rows.rename(columns=away_rename_map)
    away_rows["is_home"] = 0

    team_matches = pd.concat(
        [home_rows, away_rows],
        ignore_index=True,
    )

    output_columns = [
        "understat_id",
        "date",
        "league",
        "season",
        "team",
        "opponent",
        "is_home",
        "goals",
    ]

    if include_extra_columns:
        output_columns.append("goals_against")

        if has_xg:
            output_columns.extend(["xg_for", "xg_against"])

    team_matches = team_matches[output_columns]

    team_matches = team_matches.sort_values(
        ["date", "understat_id", "is_home"],
        ascending=[True, True, False],
    ).reset_index(drop=True)

    expected_rows = len(df) * 2

    if len(team_matches) != expected_rows:
        raise ValueError(
            f"Expected {expected_rows} team-match rows, got {len(team_matches)}."
        )

    return team_matches


def get_known_teams(team_matches: pd.DataFrame) -> set[str]:
    """
    Возвращает множество команд, встречавшихся в team-match dataframe.
    """

    if "team" not in team_matches.columns:
        raise ValueError("Column 'team' is missing.")

    return set(team_matches["team"].dropna().unique())


def get_known_leagues(team_matches: pd.DataFrame) -> set[str]:
    """
    Возвращает множество лиг, встречавшихся в team-match dataframe.
    """

    if "league" not in team_matches.columns:
        raise ValueError("Column 'league' is missing.")

    return set(team_matches["league"].dropna().unique())
