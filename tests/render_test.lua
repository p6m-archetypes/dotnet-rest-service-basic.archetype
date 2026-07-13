--- Acceptance tests for the rendered project layout and template substitution.
--- Renders the archetype with persistence=None (shared, suite-scoped) and asserts on the tree.
--- Run from the repo root: `prova tests/render_test.lua` (or just `prova`).

local h = require("tests.helpers")

prova.describe("dotnet-rest-service-basic archetype (persistence=None)", function()
  prova.test("produces the expected top-level layout", { requires = h.NEEDS_ARCHETECT }, function(t)
    local root = t:use(h.rendered_none)
    t:expect_all(function()
      t:expect(fs.exists(root), "project root"):is_true()
      t:expect(fs.exists(root .. "/" .. h.ASM .. ".sln"), "solution file"):is_true()
      t:expect(fs.exists(root .. "/Dockerfile"), "Dockerfile"):is_true()
      t:expect(fs.exists(root .. "/README.md"), "README"):is_true()
      t:expect(fs.exists(root .. "/NuGet.config"), "NuGet.config"):is_true()
      t:expect(fs.exists(root .. "/Directory.Build.props"), "Directory.Build.props"):is_true()
      t:expect(fs.exists(root .. "/.github/workflows/build.yml"), "CI build workflow"):is_true()
      t:expect(fs.exists(root .. "/.platform/kubernetes/dev"), "platform manifests"):is_true()
    end)
  end)

  prova.test("scaffolds every expected .NET module", { requires = h.NEEDS_ARCHETECT }, function(t)
    local root = t:use(h.rendered_none)
    local modules = { "API", "Client", "Core", "Server", "UnitTests", "IntegrationTests" }
    t:expect_all(function()
      for _, m in ipairs(modules) do
        local csproj = root .. "/" .. h.ASM .. "." .. m .. "/" .. h.ASM .. "." .. m .. ".csproj"
        t:expect(fs.exists(csproj), m .. " csproj"):is_true()
      end
    end)
  end)

  prova.test("wires prefix/suffix through file and assembly names", { requires = h.NEEDS_ARCHETECT }, function(t)
    local root = t:use(h.rendered_none)
    local sln = fs.read(root .. "/" .. h.ASM .. ".sln")
    -- The solution references the PascalCase assemblies (TestService.API, …), proving the
    -- {{ PrefixName }}{{ SuffixName }} placeholders rendered in both paths and contents.
    t:expect(sln, "sln references API assembly"):contains(h.ASM .. ".API")
    t:expect(sln, "sln references Server assembly"):contains(h.ASM .. ".Server")
  end)

  prova.test("renders the configured service/management ports into the Dockerfile", { requires = h.NEEDS_ARCHETECT }, function(t)
    local dockerfile = fs.read(t:use(h.rendered_none) .. "/Dockerfile")
    t:expect(dockerfile, "service port exposed"):contains("EXPOSE " .. h.SERVICE_PORT)
    t:expect(dockerfile, "management port exposed"):contains("EXPOSE " .. h.MANAGEMENT_PORT)
  end)

  prova.test("leaves no unrendered template markers", { requires = h.NEEDS_ARCHETECT }, function(t)
    local root = t:use(h.rendered_none)
    -- Scan every text file for a bare `{{` jinja marker. GitHub Actions expressions legitimately
    -- use `${{ … }}`, so those are filtered out; anything left is a template-substitution bug.
    local r = shell.run("grep -rIn '{{' " .. root .. " | grep -v '${{' || true")
    t:expect(r.stdout, "leftover template markers"):equals("")
  end)
end)
