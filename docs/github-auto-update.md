# GitHub Auto-Update (NetSparkle) - full setup

This setup publishes each new version to GitHub Releases and automatically updates `appcast.xml` via GitHub Pages (GitHub Actions deployment mode).

After one-time setup, your release flow is:
1. Create GitHub release tag (for example `v1.2.3`)
2. GitHub Action builds app, uploads ZIP asset, updates appcast
3. Clients receive update from NetSparkle automatically

## What is already in repository

- Auto-update integration in app via `NetSparkleUpdater.UI.WPF`
- GitHub Actions workflow: `.github/workflows/release-auto-update.yml`
- Key generation script: `scripts/Generate-NetSparkleKeys.ps1`
- Release helper script: `scripts/Create-GitHubRelease.ps1`
- Fallback defaults for update source: `ServerMultiTool/Model/Infrastructure/DefaultValues/DefaultUpdateSettings.cs`

## One-time setup (15-20 min)

### 1) Generate signing keys

Run:

```powershell
cd C:\Repositories\SaintZet\ServerMultiTool
powershell -ExecutionPolicy Bypass -File .\scripts\Generate-NetSparkleKeys.ps1 -OutputPath .\keys
```

You will get:
- `keys/NetSparkle_Ed25519.pub` (public key)
- `keys/NetSparkle_Ed25519.priv` (private key)

> Keep private key secret. Never commit it.

### 2) Add GitHub repository secrets

In GitHub repo settings -> **Secrets and variables** -> **Actions** add:

- `NETSPARKLE_PRIVATE_KEY` = content of `NetSparkle_Ed25519.priv`
- `NETSPARKLE_PUBLIC_KEY` = content of `NetSparkle_Ed25519.pub`

### 3) Enable GitHub Pages

In GitHub repo settings -> **Pages**:
- Source: `GitHub Actions`

Workflow will publish `appcast.xml` here:

`https://<OWNER>.github.io/<REPO>/appcast.xml`

### 4) Set default appcast URL + public key in app

Edit `ServerMultiTool/Model/Infrastructure/DefaultValues/DefaultUpdateSettings.cs`:

- `AppcastUrl` = your Pages appcast URL
- `Ed25519PublicKey` = public key string

Commit and push.

This gives zero-manual setup for end users (they get defaults even with empty local settings).

## Release process (fully automated)

### Option A - with helper script (recommended)

```powershell
cd C:\Repositories\SaintZet\ServerMultiTool
powershell -ExecutionPolicy Bypass -File .\scripts\Create-GitHubRelease.ps1 -Version 1.2.3 -TargetBranch main
```

### Option B - manually in GitHub UI

Create a new release with tag like `v1.2.3`.

---

After release is published, workflow `.github/workflows/release-auto-update.yml` does:
1. publish `ServerMultiTool` for `win-x64`
2. create ZIP artifact
3. upload ZIP into release assets
4. regenerate `appcast.xml` + signature
5. deploy appcast to GitHub Pages

## Verify that everything works

### 1) Check release artifacts

In GitHub release page, asset should include:
- `ServerMultiTool-win-x64.zip`

### 2) Check appcast URL

Open in browser:

`https://<OWNER>.github.io/<REPO>/appcast.xml`

It should contain your latest version and release asset URL.

### 3) Client app check

In app `Settings -> General`:
- `Appcast URL` populated (from defaults or saved settings)
- `Ed25519 Public Key` populated
- Click `Check for Updates Now`

If newer release exists, NetSparkle shows update UI.

## Rollback / hotfix strategy

- If release is bad: delete GitHub release + tag and publish fixed `vX.Y.Z+1`
- If appcast entry is wrong: rerun workflow with `workflow_dispatch`
- Do not rotate key pair unless required; key rotation requires shipping new public key to clients

## Security notes

- Keep `NetSparkle_Ed25519.priv` only in GitHub Secret and secure local vault
- Public key is safe to ship in app
- Never disable signature verification in production

## Troubleshooting

- **Workflow fails: missing secret**
  - Verify `NETSPARKLE_PRIVATE_KEY` and `NETSPARKLE_PUBLIC_KEY`

- **No appcast on Pages**
  - Check that GitHub Pages source is set to `GitHub Actions`

- **Client does not detect update**
  - Ensure app version increases
  - Ensure release tag version > installed version
  - Ensure appcast URL and public key match same key pair

- **Manual check button disabled**
  - Fill `Appcast URL` in settings


