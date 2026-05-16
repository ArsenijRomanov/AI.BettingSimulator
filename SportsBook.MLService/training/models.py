from __future__ import annotations

import numpy as np
import pandas as pd
from sklearn.compose import ColumnTransformer
from sklearn.linear_model import PoissonRegressor
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import OneHotEncoder

from training.evaluation import evaluate_predictions


def build_preprocessor(
    categorical_features: list[str],
    numeric_features: list[str],
) -> ColumnTransformer:
    """
    Создаёт preprocessing pipeline.

    Категориальные признаки кодируются через OneHotEncoder.
    Числовые признаки пропускаются как есть.
    """

    return ColumnTransformer(
        transformers=[
            (
                "categorical",
                OneHotEncoder(
                    handle_unknown="ignore",
                    sparse_output=False,
                ),
                categorical_features,
            ),
            (
                "numeric",
                "passthrough",
                numeric_features,
            ),
        ],
        remainder="drop",
    )


def build_poisson_pipeline(
    categorical_features: list[str],
    numeric_features: list[str],
    alpha: float,
    max_iter: int = 1000,
) -> Pipeline:
    """
    Создаёт sklearn Pipeline:

        preprocessing -> PoissonRegressor
    """

    if alpha < 0:
        raise ValueError("alpha must be non-negative.")

    preprocessor = build_preprocessor(
        categorical_features=categorical_features,
        numeric_features=numeric_features,
    )

    model = Pipeline(
        steps=[
            ("preprocessor", preprocessor),
            (
                "regressor",
                PoissonRegressor(
                    alpha=alpha,
                    max_iter=max_iter,
                ),
            ),
        ]
    )

    return model


def tune_poisson_alpha(
    X_train: pd.DataFrame,
    y_train: pd.Series,
    X_valid: pd.DataFrame,
    y_valid: pd.Series,
    alpha_values: list[float],
    categorical_features: list[str],
    numeric_features: list[str],
    max_iter: int = 1000,
) -> pd.DataFrame:
    """
    Перебирает alpha для PoissonRegressor.

    Возвращает dataframe с метриками на train и validation.
    Лучший alpha выбирается по valid_mean_poisson_deviance.
    """

    rows = []

    for alpha in alpha_values:
        model = build_poisson_pipeline(
            categorical_features=categorical_features,
            numeric_features=numeric_features,
            alpha=alpha,
            max_iter=max_iter,
        )

        model.fit(X_train, y_train)

        train_pred = model.predict(X_train)
        valid_pred = model.predict(X_valid)

        train_metrics = evaluate_predictions(
            y_true=y_train,
            y_pred=train_pred,
            dataset_name="train",
        )

        valid_metrics = evaluate_predictions(
            y_true=y_valid,
            y_pred=valid_pred,
            dataset_name="validation",
        )

        rows.append(
            {
                "alpha": alpha,
                "train_mae": train_metrics["mae"],
                "train_rmse": train_metrics["rmse"],
                "train_mean_poisson_deviance": train_metrics[
                    "mean_poisson_deviance"
                ],
                "train_min_lambda": train_metrics["min_predicted_lambda"],
                "train_max_lambda": train_metrics["max_predicted_lambda"],
                "valid_mae": valid_metrics["mae"],
                "valid_rmse": valid_metrics["rmse"],
                "valid_mean_poisson_deviance": valid_metrics[
                    "mean_poisson_deviance"
                ],
                "valid_min_lambda": valid_metrics["min_predicted_lambda"],
                "valid_max_lambda": valid_metrics["max_predicted_lambda"],
            }
        )

    return pd.DataFrame(rows).sort_values(
        "valid_mean_poisson_deviance",
        ascending=True,
    ).reset_index(drop=True)


def get_best_alpha(
    alpha_tuning_results: pd.DataFrame,
    metric: str = "valid_mean_poisson_deviance",
) -> float:
    """
    Возвращает лучший alpha из таблицы tune_poisson_alpha.
    """

    if alpha_tuning_results.empty:
        raise ValueError("alpha_tuning_results is empty.")

    if metric not in alpha_tuning_results.columns:
        raise ValueError(f"Metric column '{metric}' is missing.")

    best_row = alpha_tuning_results.sort_values(metric).iloc[0]

    return float(best_row["alpha"])


def predict_match_lambdas(
    model: Pipeline,
    home_team: str,
    away_team: str,
    league: str,
    season: int,
) -> dict[str, float]:
    """
    Делает два предсказания одной Poisson-моделью:

    - home row -> lambdaHome
    - away row -> lambdaAway
    """

    prediction_rows = pd.DataFrame(
        [
            {
                "team": home_team,
                "opponent": away_team,
                "is_home": 1,
                "league": league,
                "season": season,
            },
            {
                "team": away_team,
                "opponent": home_team,
                "is_home": 0,
                "league": league,
                "season": season,
            },
        ]
    )

    lambdas = model.predict(prediction_rows)
    lambdas = np.clip(lambdas, 1e-9, None)

    return {
        "lambdaHome": float(lambdas[0]),
        "lambdaAway": float(lambdas[1]),
    }
