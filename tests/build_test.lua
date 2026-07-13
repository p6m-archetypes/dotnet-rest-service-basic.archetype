--- The runtime tier: the generated project must actually compile. Renders persistence=None
--- (no database container needed) and runs `dotnet build` on the solution.
---
--- Gated on `dotnet` via `requires` (skips cleanly where the SDK is absent, e.g. a docs-only CI
--- job). Tagged `build` so the slow tier can be selected or excluded:
---   prova --tags build          # only the build test
--- The generated NuGet.config ships with the private Artifactory source commented out and every
--- PackageReference resolves from public nuget.org, so restore needs network but no credentials.

local h = require("tests.helpers")

prova.test("the generated solution compiles", { requires = { "archetect2", "dotnet" }, tags = { "build" }, timeout = "600s" },
  function(t)
    local root = t:use(h.rendered_none)
    local r = shell.run("dotnet build " .. h.ASM .. ".sln -c Release", {
      cwd = root,
      timeout = "600s",
    })
    t:expect(r.code, "dotnet build exit code"):equals(0)
    t:expect(r.stdout, "no build errors"):never():contains("error ")
  end)
