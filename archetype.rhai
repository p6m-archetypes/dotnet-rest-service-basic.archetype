let context = #{};

if switch_enabled("debug-answers") {
    log(Info, archetect::archetype::description() +" Answers:");
    display(as_yaml(ANSWERS));
}

// Use the same switches, defaults, etc passed to this archetype, and pass them to the next
let render_settings = archetect::archetype::settings();

context += render(Archetype("org-prompts"), ANSWERS, render_settings);

context += render(Archetype("project-prompts"), ANSWERS + #{
    "suffix_default": "Service",
    "suffix_options": [
        "Service",
        "Orchestrator",
        "Adapter",
        "Domain Gateway"
    ],
}, render_settings);

context += prompt("Persistence:", "persistence", #{
    type: Select([
        "CockroachDB",
        "None",
    ]),
    defaults_with: "CockroachDB",
});

context += prompt("Artifactory Host:", "artifactory-host", #{
  defaults_with: "p6m.jfrog.io",
});

context += set("service-port", 5030);
context += set("management-port", context["service-port"] + 1);
context += set("database-port", 26257);
context += set("debug-port", context["service-port"] + 9);

if switch_enabled("debug-context") {
    log(Info, `${archetect::archetype::description()} (${context["project-name"]}) Context:`);
    display(as_yaml(context));
}

render(Directory("contents/base"), context);

render(Archetype("manifests"), context["project-name"], context + #{
  "language": ".NET",
  "protocol": "REST",
});

//
// Publishing
//
if switch_enabled("zip") {
  zip(context["project-name"], render("{{ project-name }}.zip", context));
}
if switch_enabled("tarball") {
  tar_gz(context["project-name"], render("{{ project-name }}.tar.gz", context));
}

if switch_enabled("git-init") || switch_enabled("git-publish") {
  git_init(context["project-name"], "main");
  git_add_all(context["project-name"]);
  git_commit(context["project-name"], "initial commit");
  git_remote_add(context["project-name"], "origin", render("https://github.com/{{ org-solution-name }}/{{ project-name }}.git", context));

  if switch_enabled("git-publish") {
    gh_repo_create(render("{{ org-solution-name }}/{{ project-name }}", context), Private);
    git_push(context["project-name"], "origin", "main");
  }
} else {
  display();
  log(Info, "Copy and paste the following commands to publish your project:");
  display();

  display(render("cd {{ project-name }}", context));
  display("git init -b main");
  display(render("gh repo create {{ org-solution-name }}/{{ project-name }} --private --source=. --remote=origin", context));
  display("git add .");
  display("git commit -m 'initial commit'");
  display("git push -u origin HEAD");
}

context
