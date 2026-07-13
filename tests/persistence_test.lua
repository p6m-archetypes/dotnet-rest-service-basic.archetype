--- Acceptance tests for the conditional persistence wiring in archetype.rhai:
---   render(Directory("contents/persistence-" + lower(persistence)), …)
---   + render("contents/persistence-common") only when persistence != "None".
--- Contrasts the None and PostgreSQL renders (both suite-scoped, shared with the other files).
--- Run from the repo root: `prova tests/persistence_test.lua`.

local h = require("tests.helpers")

prova.describe("persistence wiring", function()
  prova.test("PostgreSQL adds a Persistence module, docker-compose, and a postgres service", { requires = h.NEEDS_ARCHETECT }, function(t)
    local root = t:use(h.rendered_postgres)
    t:expect_all(function()
      t:expect(fs.exists(root .. "/" .. h.ASM .. ".Persistence"), "Persistence project dir"):is_true()
      t:expect(fs.exists(root .. "/docker-compose.yml"), "docker-compose.yml"):is_true()
      local sln = fs.read(root .. "/" .. h.ASM .. ".sln")
      t:expect(sln, "sln references Persistence"):contains(h.ASM .. ".Persistence")
      local compose = fs.read(root .. "/docker-compose.yml")
      t:expect(compose, "compose declares a postgres image"):contains("postgres")
      t:expect(compose, "compose exposes 5432"):contains("5432")
    end)
  end)

  prova.test("None omits the Persistence module and docker-compose entirely", { requires = h.NEEDS_ARCHETECT }, function(t)
    local root = t:use(h.rendered_none)
    t:expect_all(function()
      t:expect(fs.exists(root .. "/" .. h.ASM .. ".Persistence"), "no Persistence project"):is_false()
      t:expect(fs.exists(root .. "/docker-compose.yml"), "no docker-compose"):is_false()
      local sln = fs.read(root .. "/" .. h.ASM .. ".sln")
      t:expect(sln, "sln has no Persistence reference"):never():contains("Persistence")
    end)
  end)
end)
