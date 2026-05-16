from __future__ import annotations

import json
from typing import Any

import numpy as np
import pandas as pd
from sklearn.metrics import mean_absolute_error, mean_poisson_deviance, mean_squared_error


def evaluate_predictions(
    y_true,
    y_pred,
    dataset_name: str,
) -> dict[str, float | int | str]:
    """
    Считает основные метрики для предсказанных лямбд.

    y_true:
        реальные голы.

    y_pred:
        предсказанные lambda.

    dataset_name:
        train / validation / test.
    """

    y_true_array = np.asarray(y_true)
    y_pred_array = np.asarray(y_pred)

    # Poisson deviance требует строго положительные предсказания.
    y_pred_array = np.clip(y_pred_array, 1e-9, None)

    mae = mean_absolute_error(y_true_array, y_pred_array)
    rmse = mean_squared_error(y_true_array, y_pred_array) ** 0.5
    poisson_deviance = mean_poisson_deviance(y_true_array, y_pred_array)

    return {
        "dataset": dataset_name,
        "mae": float(mae),
        "rmse": float(rmse),
        "mean_poisson_deviance": float(poisson_deviance),
        "mean_predicted_lambda": float(np.mean(y_pred_array)),
        "min_predicted_lambda": float(np.min(y_pred_array)),
        "max_predicted_lambda": float(np.max(y_pred_array)),
        "mean_actual_goals": float(np.mean(y_true_array)),
        "min_actual_goals": int(np.min(y_true_array)),
        "max_actual_goals": int(np.max(y_true_array)),
    }


def evaluate_experiment(
    experiment_name: str,
    y_by_split: dict[str, Any],
    predictions_by_split: dict[str, Any],
    params: dict | None = None,
    notes: str | None = None,
) -> pd.DataFrame:
    """
    Считает метрики эксперимента по нескольким split'ам.
    """

    params = params or {}

    rows = []

    for dataset_name, y_true in y_by_split.items():
        if dataset_name not in predictions_by_split:
            raise ValueError(f"Missing predictions for split '{dataset_name}'.")

        y_pred = predictions_by_split[dataset_name]

        row = evaluate_predictions(
            y_true=y_true,
            y_pred=y_pred,
            dataset_name=dataset_name,
        )

        row["experiment"] = experiment_name
        row["params"] = json.dumps(params, ensure_ascii=False)
        row["notes"] = notes

        rows.append(row)

    return pd.DataFrame(rows)


def append_experiment_results(
    experiment_results: pd.DataFrame,
    new_results: pd.DataFrame,
    replace_existing: bool = True,
) -> pd.DataFrame:
    """
    Добавляет результаты эксперимента в общий dataframe.

    Если replace_existing=True, то при повторном запуске ячейки старые строки
    с тем же experiment удаляются.
    """

    if new_results.empty:
        return experiment_results

    required_columns = {"experiment", "dataset"}
    missing_columns = sorted(required_columns - set(new_results.columns))

    if missing_columns:
        raise ValueError(f"new_results is missing columns: {missing_columns}")

    result = experiment_results.copy()

    if replace_existing and not result.empty:
        experiments_to_replace = set(new_results["experiment"])
        result = result[~result["experiment"].isin(experiments_to_replace)].copy()

    result = pd.concat(
        [result, new_results],
        ignore_index=True,
    )

    return result


def show_experiment_results(
    experiment_results: pd.DataFrame,
    datasets: list[str] | None = None,
    sort_by: str = "mean_poisson_deviance",
    detailed: bool = False,
) -> pd.DataFrame:
    """
    Показывает dataframe с результатами экспериментов.
    """

    if experiment_results.empty:
        return experiment_results

    result = experiment_results.copy()

    if datasets is not None:
        result = result[result["dataset"].isin(datasets)].copy()

    compact_columns = [
        "experiment",
        "dataset",
        "mae",
        "rmse",
        "mean_poisson_deviance",
        "mean_predicted_lambda",
        "min_predicted_lambda",
        "max_predicted_lambda",
        "mean_actual_goals",
    ]

    detailed_columns = compact_columns + [
        "min_actual_goals",
        "max_actual_goals",
        "params",
        "notes",
    ]

    columns_order = detailed_columns if detailed else compact_columns

    existing_columns = [column for column in columns_order if column in result.columns]
    result = result[existing_columns]

    return result.sort_values(
        ["dataset", sort_by],
        ascending=[True, True],
    ).reset_index(drop=True)


def calculate_relative_improvement(
    baseline_score: float,
    candidate_score: float,
) -> float:
    """
    Считает относительное улучшение candidate относительно baseline в процентах.
    """

    if baseline_score == 0:
        raise ValueError("baseline_score cannot be zero.")

    return (baseline_score - candidate_score) / baseline_score * 100


def get_metric(
    experiment_results: pd.DataFrame,
    experiment: str,
    dataset: str,
    metric: str = "mean_poisson_deviance",
) -> float:
    """
    Достаёт одну метрику из experiment_results.
    """

    values = experiment_results.loc[
        (experiment_results["experiment"] == experiment)
        & (experiment_results["dataset"] == dataset),
        metric,
    ]

    if values.empty:
        raise ValueError(
            f"Metric not found: experiment={experiment}, dataset={dataset}, metric={metric}"
        )

    return float(values.iloc[0])
