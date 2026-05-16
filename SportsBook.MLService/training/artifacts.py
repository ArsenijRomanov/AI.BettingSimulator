from __future__ import annotations

import json
from pathlib import Path
from typing import Any

import joblib
import pandas as pd


def ensure_directory(path: Path) -> None:
    """
    Создаёт директорию, если её ещё нет.
    """

    path.mkdir(parents=True, exist_ok=True)


def save_model(model: Any, path: Path) -> None:
    """
    Сохраняет sklearn/joblib модель.
    """

    ensure_directory(path.parent)
    joblib.dump(model, path)


def load_model(path: Path) -> Any:
    """
    Загружает sklearn/joblib модель.
    """

    if not path.exists():
        raise FileNotFoundError(f"Model artifact not found: {path}")

    return joblib.load(path)


def save_json(data: dict, path: Path) -> None:
    """
    Сохраняет dict в JSON.
    """

    ensure_directory(path.parent)

    with path.open("w", encoding="utf-8") as file:
        json.dump(data, file, ensure_ascii=False, indent=2)


def load_json(path: Path) -> dict:
    """
    Загружает JSON как dict.
    """

    if not path.exists():
        raise FileNotFoundError(f"JSON file not found: {path}")

    with path.open("r", encoding="utf-8") as file:
        return json.load(file)


def save_results(results: pd.DataFrame, path: Path) -> None:
    """
    Сохраняет таблицу результатов экспериментов в CSV.
    """

    ensure_directory(path.parent)
    results.to_csv(path, index=False)


def load_results(path: Path) -> pd.DataFrame:
    """
    Загружает таблицу результатов экспериментов из CSV.
    """

    if not path.exists():
        raise FileNotFoundError(f"Results file not found: {path}")

    return pd.read_csv(path)
