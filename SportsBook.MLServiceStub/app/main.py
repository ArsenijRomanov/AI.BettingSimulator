from __future__ import annotations

import hashlib
from datetime import datetime
from typing import Any

from fastapi import FastAPI
from pydantic import BaseModel, Field


app = FastAPI(
    title="SportsBook ML Service",
    version="0.1.0",
    description="Temporary ML stub for predicting expected goals/lambdas.",
)


class LambdaPredictionRequest(BaseModel):
    homeTeamName: str = Field(min_length=1, max_length=128)
    awayTeamName: str = Field(min_length=1, max_length=128)
    competition: str | None = Field(default=None, max_length=128)
    startTime: datetime | None = None


class LambdaPredictionResponse(BaseModel):
    lambdaHome: float
    lambdaAway: float
    modelVersion: str
    isStub: bool
    debug: dict[str, Any]


TEAM_RATINGS: dict[str, float] = {
    "arsenal": 1.18,
    "chelsea": 1.05,
    "manchester city": 1.25,
    "man city": 1.25,
    "liverpool": 1.20,
    "tottenham": 1.08,
    "manchester united": 1.07,
    "man united": 1.07,
    "newcastle": 1.04,
    "real madrid": 1.28,
    "barcelona": 1.22,
    "bayern munich": 1.27,
    "psg": 1.21,
    "inter": 1.15,
    "milan": 1.09,
    "juventus": 1.10,
}


def normalize_team_name(value: str) -> str:
    return " ".join(value.strip().lower().split())


def deterministic_noise(home: str, away: str) -> float:
    key = f"{home}|{away}".encode("utf-8")
    digest = hashlib.sha256(key).hexdigest()

    number = int(digest[:8], 16)

    # Диапазон примерно [-0.10; +0.10]
    return ((number % 201) - 100) / 1000.0


def get_rating(team_name: str) -> float:
    normalized = normalize_team_name(team_name)

    if normalized in TEAM_RATINGS:
        return TEAM_RATINGS[normalized]

    # Для неизвестных команд делаем стабильный fallback.
    # Одна и та же команда всегда получит примерно один и тот же рейтинг.
    digest = hashlib.sha256(normalized.encode("utf-8")).hexdigest()
    number = int(digest[:8], 16)

    # Диапазон примерно [0.90; 1.10]
    return 0.90 + (number % 201) / 1000.0


def clamp(value: float, min_value: float = 0.2, max_value: float = 4.5) -> float:
    return max(min_value, min(max_value, value))


@app.get("/health")
def health() -> dict[str, str]:
    return {
        "status": "ok",
        "service": "SportsBook.MLService",
        "modelVersion": "stub-lambda-v1",
    }


@app.post("/predict-lambdas", response_model=LambdaPredictionResponse)
def predict_lambdas(request: LambdaPredictionRequest) -> LambdaPredictionResponse:
    home = normalize_team_name(request.homeTeamName)
    away = normalize_team_name(request.awayTeamName)

    home_rating = get_rating(home)
    away_rating = get_rating(away)

    base_total_goals = 2.65
    home_advantage = 0.18
    noise = deterministic_noise(home, away)

    strength_sum = home_rating + away_rating

    home_share = home_rating / strength_sum
    away_share = away_rating / strength_sum

    lambda_home = base_total_goals * home_share + home_advantage + noise
    lambda_away = base_total_goals * away_share - noise

    lambda_home = round(clamp(lambda_home), 3)
    lambda_away = round(clamp(lambda_away), 3)

    return LambdaPredictionResponse(
        lambdaHome=lambda_home,
        lambdaAway=lambda_away,
        modelVersion="stub-lambda-v1",
        isStub=True,
        debug={
            "homeTeamNormalized": home,
            "awayTeamNormalized": away,
            "homeRating": round(home_rating, 3),
            "awayRating": round(away_rating, 3),
            "baseTotalGoals": base_total_goals,
            "homeAdvantage": home_advantage,
            "noise": round(noise, 3),
        },
    )
