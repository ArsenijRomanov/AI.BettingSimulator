from __future__ import annotations

import pandas as pd


def split_by_seasons(
    df: pd.DataFrame,
    train_seasons: list[int],
    valid_seasons: list[int],
    test_seasons: list[int],
) -> tuple[pd.DataFrame, pd.DataFrame, pd.DataFrame]:
    """
    Делит dataframe на train / validation / test по сезонам.

    Для спортивных данных это лучше random split, потому что модель должна
    учиться на прошлом и предсказывать будущее.
    """

    if "season" not in df.columns:
        raise ValueError("Column 'season' is missing.")

    train_df = df[df["season"].isin(train_seasons)].copy()
    valid_df = df[df["season"].isin(valid_seasons)].copy()
    test_df = df[df["season"].isin(test_seasons)].copy()

    if train_df.empty:
        raise ValueError(f"Train split is empty. train_seasons={train_seasons}")

    if valid_df.empty:
        raise ValueError(f"Validation split is empty. valid_seasons={valid_seasons}")

    if test_df.empty:
        raise ValueError(f"Test split is empty. test_seasons={test_seasons}")

    return train_df, valid_df, test_df


def make_xy(
    df: pd.DataFrame,
    feature_columns: list[str],
    target_column: str = "goals",
) -> tuple[pd.DataFrame, pd.Series]:
    """
    Делает X и y из dataframe.

    X — признаки.
    y — target.
    """

    missing_features = sorted(set(feature_columns) - set(df.columns))

    if missing_features:
        raise ValueError(f"Missing feature columns: {missing_features}")

    if target_column not in df.columns:
        raise ValueError(f"Target column '{target_column}' is missing.")

    X = df[feature_columns].copy()
    y = df[target_column].copy()

    return X, y


def get_split_summary(
    train_df: pd.DataFrame,
    valid_df: pd.DataFrame,
    test_df: pd.DataFrame,
) -> pd.DataFrame:
    """
    Возвращает короткую таблицу с количеством строк по split'ам.
    """

    return pd.DataFrame(
        [
            {"dataset": "train", "rows": len(train_df)},
            {"dataset": "validation", "rows": len(valid_df)},
            {"dataset": "test", "rows": len(test_df)},
        ]
    )


def get_league_distribution_by_split(
    train_df: pd.DataFrame,
    valid_df: pd.DataFrame,
    test_df: pd.DataFrame,
) -> pd.DataFrame:
    """
    Показывает распределение лиг по train / validation / test.
    """

    return pd.DataFrame(
        {
            "train": train_df["league"].value_counts(),
            "validation": valid_df["league"].value_counts(),
            "test": test_df["league"].value_counts(),
        }
    ).fillna(0).astype(int)


def get_unknown_teams_report(
    train_df: pd.DataFrame,
    valid_df: pd.DataFrame,
    test_df: pd.DataFrame,
) -> dict:
    """
    Показывает, какие команды из validation/test отсутствуют в train.
    """

    train_teams = set(train_df["team"])
    valid_teams = set(valid_df["team"])
    test_teams = set(test_df["team"])

    valid_unknown = valid_teams - train_teams
    test_unknown = test_teams - train_teams

    return {
        "train_teams_count": len(train_teams),
        "validation_teams_count": len(valid_teams),
        "validation_unknown_teams_count": len(valid_unknown),
        "validation_unknown_teams": sorted(valid_unknown),
        "test_teams_count": len(test_teams),
        "test_unknown_teams_count": len(test_unknown),
        "test_unknown_teams": sorted(test_unknown),
    }
