--- Shared fixtures/helpers for the prova acceptance suite (a `conftest`-style module).
--- Required by the sibling `*_test.lua` files as `require("tests.helpers")`.
---
--- Why we shell out to `archetect2` instead of prova's in-process `archetect.render`:
--- this is a Gen-1 (Archetect 2.0, Rhai) archetype, and prova's `archetect` plugin only
--- renders Gen-2 (Lua) archetypes in-process. So we drive the legacy `archetect2` CLI via
--- `shell.run` and then assert on the rendered filesystem with the `fs` module. This is
--- squarely prova's black-box acceptance model: bring the system into existence, then poke it.

local M = {}

-- Archetype source. Defaults to the CWD (run `prova` from the repo root); override with
-- ARCHETYPE_SRC to point elsewhere.
M.SRC = os.getenv("ARCHETYPE_SRC") or "."

-- Fixed answers used for every render. `prefix-name`/`suffix-name` => project dir `test-service`
-- and PascalCase assembly prefix `TestService`. `service-port` MUST be an unquoted integer or the
-- Rhai `prompt(..., type: Int)` rejects it.
M.PREFIX = "test"
M.SUFFIX = "service"
M.PROJECT_DIR = M.PREFIX .. "-" .. M.SUFFIX -- "test-service"
M.ASM = "TestService"                        -- PascalCase({{prefix}}{{suffix}}) assembly prefix
M.SERVICE_PORT = 8080
M.MANAGEMENT_PORT = 8081                      -- derived in archetype.rhai as service-port + 1

-- Render the archetype headlessly for the given persistence choice ("None"|"PostgreSQL"|
-- "MySQL"|"MSSQL") into a scratch dir owned by `ctx`. Returns the absolute path to the
-- generated project root (…/test-service). Raises with archetect2's stderr on failure.
function M.render(ctx, persistence)
  local dest = ctx:tempdir()
  local answer_file = dest .. "/answers.yaml"
  fs.write(answer_file, table.concat({
    'author_full: "Prova Test <prova@example.com>"',
    'org-name: "testorg"',
    'solution-name: "testsolution"',
    'prefix-name: "' .. M.PREFIX .. '"',
    'suffix-name: "' .. M.SUFFIX .. '"',
    'persistence: "' .. persistence .. '"',
    'artifactory-host: "test.jfrog.io"',
    "service-port: " .. M.SERVICE_PORT,
    "",
  }, "\n"))

  local out = dest .. "/out"
  local cmd = table.concat({
    "archetect2 render", M.SRC, out,
    "-A", answer_file,
    "--use-defaults-all",
  }, " ")

  local r = shell.run(cmd, { timeout = "180s" })
  if r.code ~= 0 then
    error("archetype render failed (persistence=" .. persistence .. "):\n"
      .. r.stdout .. "\n" .. r.stderr)
  end
  return out .. "/" .. M.PROJECT_DIR
end

-- Suite-scoped renders shared across every test file: each persistence variant is generated
-- exactly once per `prova` run. Fixtures are lazy, so tests that `requires = { "archetect2" }`
-- and get skipped never trigger a render.
M.rendered_none = prova.fixture("rendered_none", Scope.Suite,
  function(ctx) return M.render(ctx, "None") end)

M.rendered_postgres = prova.fixture("rendered_postgres", Scope.Suite,
  function(ctx) return M.render(ctx, "PostgreSQL") end)

-- Standard gate for every test that renders: skip (not fail) where archetect2 is unavailable.
M.NEEDS_ARCHETECT = { "archetect2" }

return M
