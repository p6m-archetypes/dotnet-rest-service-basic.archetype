# Prova acceptance tests

Black-box acceptance tests for this archetype, driven by [**prova**](https://github.com/prova-rs/prova)
- a programmable, language-agnostic acceptance-test runner (Lua + a fixture model, single static
binary). Each test **renders the archetype into a throwaway directory and asserts on the result**:
the project layout, template substitution, conditional persistence wiring, and a real `dotnet build`.

This suite replaces the previous pytest [archetype-test-harness](https://github.com/p6m-archetypes/archetype-test-harness)
(`tests/manifest.yaml` + `answers/`) - the same render/layout/persistence/build checks, expressed as
self-contained prova Lua rather than data driving an external Python package. It is the harness CI runs
(see [`.github/workflows/test.yaml`](../.github/workflows/test.yaml)).

## Prerequisites

| Tool         | Needed for                    | Notes |
|--------------|-------------------------------|-------|
| `prova`      | running the suite             | `brew tap prova-rs/tap && brew install prova` |
| `archetect2` | rendering (Archetect **2.x**) | This is a Gen-1 **Rhai** archetype; prova's in-process `archetect.render` only handles Gen-2 Lua archetypes, so the tests shell out to `archetect2`. Install v2 alongside v3: `brew install archetect/tap/archetect@2 && ln -s /opt/homebrew/opt/archetect@2/bin/archetect /opt/homebrew/bin/archetect2`. Rendering tests are **skipped** (not failed) when `archetect2` is absent. |
| `dotnet`     | the `build` test only         | SDK 8+. Restores from public nuget.org (no Artifactory credentials needed - the generated `NuGet.config` ships with the private source commented out). Skipped when `dotnet` is absent. |
| network      | rendering + `dotnet restore`  | The archetype composes remote components (`org-prompts`, `project-prompts`, `manifests`, `gitignore`) that `archetect2` fetches over git and caches after the first render. |

## Running

Run **from the archetype repo root** (the render source defaults to the current directory):

```sh
prova                    # whole suite, via ./prova.toml
prova --profile smoke    # render + persistence only (skips the slow dotnet build)
prova --profile ci       # JSON (JSONL) output for CI

prova acceptance/render_test.lua   # a single file
prova --list                       # list tests without running
```

If you must run from elsewhere, point the tests at the archetype: `ARCHETYPE_SRC=/path/to/repo prova …`.

## Layout

```
prova.toml            # suite manifest (at repo root): default / smoke / ci profiles
acceptance/
  helpers.lua         # shared render helper + suite-scoped render fixtures (conftest-style)
  render_test.lua     # top-level layout, .NET modules, prefix/suffix + port substitution, no leftover {{markers}}
  persistence_test.lua# None vs PostgreSQL: conditional Persistence module + docker-compose wiring
  build_test.lua      # `dotnet build` of the generated solution (tag: build)
```

`helpers.lua` renders each persistence variant **once per run** (suite-scoped fixtures) and shares
the rendered tree across every test file, so the whole suite pays for at most two renders. It has no
`_test.lua` suffix, so the runner won't collect it as tests.

## How it works

prova's `archetect` plugin renders Gen-2 (Lua) archetypes in-process, but this archetype is Gen-1
(Rhai). So `helpers.lua` writes a YAML answers file to a scratch dir and shells out to `archetect2`
via `shell.run`, then the tests assert on the rendered filesystem with the `fs` module - which is
exactly prova's black-box model: bring the system into existence, then poke it.

## Adding a test

```lua
local h = require("acceptance.helpers")

prova.test("my new check", { requires = h.NEEDS_ARCHETECT }, function(t)
  local root = t:use(h.rendered_none)          -- or h.rendered_postgres
  t:expect(fs.read(root .. "/README.md")):contains("something")
end)
```

Gate every rendering test with `{ requires = h.NEEDS_ARCHETECT }` so it skips cleanly where
`archetect2` is unavailable. Use `t:expect_all(...)` to report every failed file check at once
rather than stopping at the first.
