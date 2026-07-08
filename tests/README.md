# Archetype integration tests

Pytest harness that validates this archetype end to end: it renders the archetype
headlessly with a known answers file, checks the generated project (expected files
present, persistence-specific files present/absent per case, no unrendered
`{{ placeholder }}` tokens left in paths or contents, generated YAML parses), and
then builds the generated .NET solution and runs its unit tests.

Two cases are covered: `postgresql` (the default persistence choice) and
`persistence-none` (no Persistence project, no docker-compose stack).

## Prerequisites

- [archetect](https://archetect.github.io/) **2.x** - this archetype uses a Rhai
  script, which archetect 3.x refuses to render. Install v2 alongside v3:

  ```sh
  brew install archetect/tap/archetect@2
  ln -s /opt/homebrew/opt/archetect@2/bin/archetect /opt/homebrew/bin/archetect2
  ```

  The harness looks for `$ARCHETECT2`, then `archetect2` on PATH, then the homebrew
  keg, then `archetect` itself if it reports a 2.x version.
- [uv](https://docs.astral.sh/uv/) on PATH (`brew install uv`) - installs Python and
  test dependencies automatically on first run
- Network access to GitHub - the archetype composes prompt/manifest components from
  public `p6m-archetypes` and `archetect-common` git sources; archetect clones and
  caches them on first render
- .NET SDK for the build tier (optional - build tests skip with a notice when
  `dotnet` is not on PATH). The generated project targets net8.0; the harness sets
  `DOTNET_ROLL_FORWARD=Major` so a newer SDK works too.

## Running

All commands run from this `tests/` directory:

```sh
uv run pytest                  # everything: render, static checks, dotnet build + test
uv run pytest -m "not build"   # fast tier only: render + static checks (no dotnet needed)
uv run pytest -m build         # build tier only
uv run pytest --offline        # don't hit the network; use archetect's cached components
uv run pytest -v -ra           # verbose, with skip/fail reasons
```

The first run clones the composed component repos and resolves Python dependencies,
so it is slower; subsequent runs use archetect's and uv's caches.

## CI

The same suite runs in GitHub Actions via
[.github/workflows/test.yaml](../.github/workflows/test.yaml) on every pull request
and push. The workflow installs archetect 2.x from its release binaries as
`archetect2`. On failure it uploads the rendered project as a build artifact.

## Inspecting rendered output

Each test session renders into a pytest temp directory, e.g.
`/tmp/pytest-of-<user>/pytest-<N>/render-postgresql0/`. Pytest keeps the last 3 runs,
so after a failure you can open the generated project from the failing run directly.

## Adding a test case

Add an entry to [manifest.yaml](manifest.yaml) plus an answers file under
[answers/](answers/) - no test code changes needed. Each case declares the answers
file, the expected project directory name, files that must exist (and, optionally,
must not exist), and the build steps to run inside the generated project. Prompt keys
in answers files are kebab-case (`org-name`, `prefix-name`, ...) except `author_full`;
anything omitted falls back to the prompt's default via `archetect render -D`.
