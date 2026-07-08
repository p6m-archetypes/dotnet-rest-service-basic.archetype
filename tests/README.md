# Archetype integration tests

This archetype is tested by the shared
[archetype-test-harness](https://github.com/p6m-archetypes/archetype-test-harness):
it renders the archetype headlessly with the answers in [answers/](answers/), checks
the generated project (expected files present, persistence-specific files
present/absent per case, no unrendered `{{ placeholder }}` tokens, generated YAML
parses), and builds the generated .NET solution and runs its unit tests. This
directory holds only data - the test code lives in the harness repo.

Two cases are covered: `postgresql` (the default persistence choice) and
`persistence-none` (no Persistence project, no docker-compose stack).

## Prerequisites

- [archetect](https://archetect.github.io/) **2.x** - this archetype uses a Rhai
  script, which archetect 3.x refuses to render. Install v2 alongside v3:

  ```sh
  brew install archetect/tap/archetect@2
  ln -s /opt/homebrew/opt/archetect@2/bin/archetect /opt/homebrew/bin/archetect2
  ```

  The harness reads `requires.archetect` from [archetype.yaml](../archetype.yaml)
  and looks for `$ARCHETECT2`, then `archetect2` on PATH, then the homebrew keg.
- [uv](https://docs.astral.sh/uv/) on PATH (`brew install uv`)
- Network access to GitHub - the archetype composes prompt/manifest components from
  git sources; archetect caches them after the first render
- .NET SDK for the build tier (optional - build tests skip with a notice when
  `dotnet` is not on PATH). The generated project targets net8.0; the harness sets
  `DOTNET_ROLL_FORWARD=Major` so newer SDKs work.

## Running

From the repo root (or this directory):

```sh
# in the flat org checkout, against the sibling harness:
uvx --from ../archetype-test-harness archetype-test

# anywhere, against the published harness:
uvx --from git+https://github.com/p6m-archetypes/archetype-test-harness@dev archetype-test
```

Extra arguments pass through to pytest:

```sh
archetype-test -m "not build"   # fast tier only: render + static checks, no dotnet needed
archetype-test -k postgresql    # single case
archetype-test --offline        # use archetect's cached component sources
archetype-test -v -ra           # verbose, with skip/fail reasons
```

## CI

[.github/workflows/test.yaml](../.github/workflows/test.yaml) calls the harness's
reusable workflow with `archetect-version: 2.1.2` (installed as `archetect2`) on
every pull request, push, and manual dispatch. On failure it uploads the rendered
project as a build artifact.

## Adding a test case

Add an entry to [manifest.yaml](manifest.yaml) plus an answers file under
[answers/](answers/) - the schema is documented in the
[harness README](https://github.com/p6m-archetypes/archetype-test-harness#manifestyaml-schema).

## Inspecting rendered output

Each run renders into a pytest temp directory, e.g.
`/tmp/pytest-of-<user>/pytest-<N>/render-postgresql0/`. Pytest keeps the last 3
runs, so after a failure you can open the generated project from the failing run.
