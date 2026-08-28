# Working with multiple AI tools on this repo

This project gets worked on by more than one AI coding tool (Claude, Cursor,
possibly Gemini). That's fine, but Unity projects have one specific hazard
that normal code doesn't: **the scene file** (`Assets/_Project/Scenes/Main.unity`)
is a huge auto-generated file. If two tools change it independently before
either one pushes, Git usually can't merge the result -- one side's changes
just get lost, silently. This bit us once already (a shader/material fix got
orphaned on a side branch for hours while another tool kept building on
`main` without it).

## Rules that avoid this

1. **One tool drives at a time.** Before switching from one AI tool to
   another, have the one you're leaving commit and push its work first.
2. **Whichever tool you're about to use, tell it to pull first.** Literally
   say "pull the latest changes from `main` before doing anything" at the
   start of a session. Don't assume it knows another tool touched the repo
   since its own last look.
3. **Never have two tools' Unity Editor windows open on this project at the
   same time.** Only one Editor instance should have the project open,
   ever. A second Editor instance opening the same project is the single
   biggest risk to the scene file.
4. **Commit and push often, in small chunks** -- not one giant change at
   the end of a long session. Small commits are the only thing that makes
   it possible to untangle a conflict if one happens anyway.
5. **If a tool reports a merge conflict**, don't let it guess -- have it
   show you both versions of the conflicting file, or stop and ask
   (Claude will do this automatically; tell other tools to do the same).

## Current state as of writing

- `main` on GitHub is the single source of truth. Everything merges into it.
- Full architecture and status: see `README.md` in this same folder.
