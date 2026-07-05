# StreamHub

A simple video-on-demand streaming platform built with ASP.NET Core MVC + EF Core + Identity.
Built for a college project — not a production streaming service (no transcoding/adaptive
bitrate), but full-featured enough to demo convincingly: accounts, upload, browse/search,
category filters, and a real HTML5 player with seek support and "continue watching."

## Features

- Register / log in (ASP.NET Core Identity)
- Upload videos with title, description, category, and thumbnail
- Browse + search + filter by category
- Video player page (native `<video>` tag — range-request seeking works out of the box
  via ASP.NET Core's static file middleware, no custom streaming code needed)
- Continue Watching: playback position is saved automatically and resumed next time

## Running locally

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
cd StreamHub
dotnet restore
dotnet run
```

Then open the URL shown in the terminal (something like `https://localhost:5001`).

No database setup needed — it uses SQLite locally (`streamhub.db`, created automatically
on first run) and switches to PostgreSQL automatically in production if a `DATABASE_URL`
environment variable is present (see below).

## Deploying to Render

### 1. Push this project to GitHub

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin <your-repo-url>
git push -u origin main
```

### 2. Create a PostgreSQL database on Render

1. Render dashboard → **New** → **PostgreSQL**
2. Give it a name, choose the free plan, create it
3. Once created, copy the **Internal Database URL** shown on the database's page

### 3. Create the web service

1. Render dashboard → **New** → **Web Service**
2. Connect your GitHub repo
3. Render will detect the `Dockerfile` automatically — choose **Docker** as the environment
4. Under **Environment Variables**, add:
   - Key: `DATABASE_URL`
   - Value: (paste the Internal Database URL from step 2)
5. Deploy

Render builds the Docker image and gives you a live HTTPS URL. The app creates its database
tables automatically on first startup (via `EnsureCreated()` — no migration commands needed).

### Notes for your demo / report

- **Free tier cold starts:** Render's free web services spin down after 15 minutes of
  inactivity and take ~30–60 seconds to wake up on the next request. Worth mentioning if
  a professor tests it cold.
- **File storage:** uploaded videos are saved to disk on the running container. Render's
  free tier has an *ephemeral* filesystem — files survive as long as the container keeps
  running, but are wiped on redeploy or restart. For a demo this is fine; for anything
  longer-lived, the natural upgrade is Render's persistent Disks add-on or an object store
  like Cloudinary/Backblaze B2.
- **Password rules** are deliberately relaxed (6-char minimum, no complexity requirements)
  to keep demo account creation fast — tighten these in `Program.cs` if your grading
  criteria expect stronger validation.
