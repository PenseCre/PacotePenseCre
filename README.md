# Pacote Pense & Cre

Open **File > Build with PenseCre** to configure scenes and build output.

- **Build** creates the player without archiving or installer generation.
- **Build + Archive** builds the player, then applies the configured ZIP and installer options.
- **Archive** applies those options to existing build output.

When **Archive to Zip** is enabled, ZIP creation runs in the background after foreground build, output cleanup, and installer steps finish. The editor remains available. One-build-per-scene mode produces a sequential batch of ZIPs. Existing ZIP names and destinations are preserved; a completed ZIP is never overwritten.

The build window shows the current archive, phase/file, processed file/byte counts for that ZIP (reset for each action), and its queue position, such as 2/4. These are activity counters, not an estimated completion percentage. Close and reopen the window to reconnect to the same ZIP operation.

Choose **Cancel ZIP** to stop unfinished zipping. The operation closes its files, removes its own temporary `.partial` output, and skips queued ZIPs. Uncompressed build output and ZIPs already completed remain available. An archive finishing publication as cancellation is requested may still complete; it is retained. If cleanup fails, the error identifies the leftover file to close/remove before rebuilding.

Requesting another Build, Build + Archive, or Archive while zipping first opens a confirmation. **Cancel ZIP and Build** (or Archive) cancels unfinished work and waits for partial-file cleanup before starting the requested action exactly once. **Keep Zipping**, Escape, or dismissing the dialog abandons the new action before its build side effects start. Closing the window while that new action is waiting discards the pending action.

When Unity reports script compilation starting, active zipping is cancelled. Assembly reload is deferred until worker cleanup finishes. Unity may defer the compilation itself while the ZIP owns its reload lock; use Cancel ZIP to apply pending script changes sooner. Quitting during zipping first requests cancellation and defers that quit attempt: retry normal quit when cleanup is finished, preserving Unity's regular save prompts. Forced termination cannot guarantee cleanup.

## Programmatic editor usage

The existing `Build.Zip(...)` signature now schedules background work and returns promptly. Returning no longer means the ZIP exists. Observe `BuildZipOperation.IsActive` and `BuildZipOperation.Status` on the editor main thread for completion, cancellation, or failure; `Changed` notifications also run on the main thread. `BuildZipOperation.Cancel()` requests non-blocking cancellation. Direct conflicting `Build.Zip` / `Build.RunBuild` calls reject the conflict without silently cancelling another operation or displaying an interactive prompt.

Paths/configuration are captured before dispatch. Worker code reads ordinary files only; editing build output manually while it is being zipped is unsupported. Symbolic links/reparse points and directory depth beyond 128 fail explicitly rather than following cycles or silently omitting files. Very slow or stalled storage can delay cancellation beyond the normal local-storage target.

This change adds no packages, assembly definitions, or runtime settings. It uses the existing editor assemblies and framework ZIP/task APIs. The package's declared Unity minimum remains unchanged; legacy-version compatibility must be verified separately from Unity 6000.3.14f1 validation.
