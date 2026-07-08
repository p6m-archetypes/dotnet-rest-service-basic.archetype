"""Shared fixtures: load manifest.yaml, render each case once per session.

This archetype is Archetect 2.x (Rhai). Archetect 3.x refuses .rhai scripts, so the
harness resolves a v2 binary: $ARCHETECT2 if set, then `archetect2` on PATH, then the
homebrew keg (archetect/tap/archetect@2), then `archetect` itself if it reports 2.x.
"""

import shutil
import subprocess
from dataclasses import dataclass, field
from functools import lru_cache
from pathlib import Path

import os
import pytest
import yaml

TESTS_DIR = Path(__file__).parent
ARCHETYPE_ROOT = TESTS_DIR.parent
RENDER_TIMEOUT = 300  # seconds; includes cloning component sources on a cold cache


@lru_cache(maxsize=1)
def find_archetect2() -> str:
    candidates = [
        os.environ.get("ARCHETECT2"),
        shutil.which("archetect2"),
        "/opt/homebrew/opt/archetect@2/bin/archetect",
        "/usr/local/opt/archetect@2/bin/archetect",
        shutil.which("archetect"),
    ]
    for candidate in candidates:
        if not candidate or not (shutil.which(candidate) or Path(candidate).is_file()):
            continue
        result = subprocess.run([candidate, "--version"], capture_output=True, text=True)
        if result.returncode == 0 and result.stdout.split()[-1].startswith("2."):
            return candidate
    pytest.fail(
        "No Archetect 2.x binary found. This archetype uses a Rhai script, which "
        "Archetect 3.x does not render. Install v2 alongside v3: "
        "`brew install archetect/tap/archetect@2`, then either symlink it as "
        "`archetect2` or point $ARCHETECT2 at the binary."
    )


@dataclass
class Case:
    name: str
    answers: Path
    project_dir: str
    expected_files: list[str] = field(default_factory=list)
    absent_files: list[str] = field(default_factory=list)
    requires: list[str] = field(default_factory=list)
    build_steps: list[list[str]] = field(default_factory=list)
    env: dict[str, str] = field(default_factory=dict)
    yaml_globs: list[str] = field(default_factory=list)


def load_cases() -> list[Case]:
    manifest = yaml.safe_load((TESTS_DIR / "manifest.yaml").read_text())
    return [
        Case(
            name=raw["name"],
            answers=TESTS_DIR / raw["answers"],
            project_dir=raw["project_dir"],
            expected_files=raw.get("expected_files", []),
            absent_files=raw.get("absent_files", []),
            requires=raw.get("requires", []),
            build_steps=raw.get("build_steps", []),
            env={k: str(v) for k, v in raw.get("env", {}).items()},
            yaml_globs=raw.get("yaml_globs", []),
        )
        for raw in manifest["cases"]
    ]


def pytest_addoption(parser):
    parser.addoption(
        "--offline",
        action="store_true",
        help="pass --offline to archetect (use only already-cached component sources)",
    )


@pytest.fixture(scope="session", params=load_cases(), ids=lambda c: c.name)
def case(request) -> Case:
    return request.param


@pytest.fixture(scope="session")
def rendered_project(case: Case, tmp_path_factory, request) -> Path:
    """Render the archetype headlessly for this case; returns the generated project dir."""
    archetect = find_archetect2()

    out_dir = tmp_path_factory.mktemp(f"render-{case.name}")
    # v2 takes the destination as a positional argument (no --dest flag)
    cmd = [
        archetect, "render", str(ARCHETYPE_ROOT), str(out_dir),
        "-A", str(case.answers),
        "-D",  # use prompt defaults for anything the answers file doesn't cover
        "--headless",
    ]
    if request.config.getoption("--offline"):
        cmd.append("--offline")

    result = subprocess.run(cmd, capture_output=True, text=True, timeout=RENDER_TIMEOUT)
    if result.returncode != 0:
        pytest.fail(
            f"archetect render failed (exit {result.returncode})\n"
            f"command: {' '.join(cmd)}\n"
            f"--- stdout ---\n{result.stdout}\n--- stderr ---\n{result.stderr}"
        )

    project = out_dir / case.project_dir
    if not project.is_dir():
        rendered = [p.name for p in out_dir.iterdir()]
        pytest.fail(
            f"render succeeded but expected project dir {case.project_dir!r} is missing; "
            f"rendered top-level entries: {rendered}"
        )
    return project
