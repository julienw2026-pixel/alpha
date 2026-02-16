# Meeting Scribe (Windows)

Windows desktop app for meeting audio recording (mic), offline transcription (whisper.cpp), and online summarization via an Anthropic Messages compatible API (your proxy).

## What You Get (V1)
- Record microphone audio to WAV
- Run offline transcription via `whisper.cpp`
- Call LLM API to generate:
  - Summary (bullets)
  - Decisions
  - Action items
  - Open questions
- Save a Markdown report per meeting

## Build & Release (recommended)
This repo includes GitHub Actions that builds the Windows app and publishes a ZIP artifact.

## Configuration
DO NOT hardcode keys.

Set env vars on Windows (User env vars):
- `MEETING_SCRIBE_BASE_URL` = `https://nexus.itssx.com/api/claude_code/cc_minimax21`
- `MEETING_SCRIBE_API_KEY` = (your key)
- `MEETING_SCRIBE_API_KIND` = `anthropic-messages`
- `MEETING_SCRIBE_MODEL` = `claude-sonnet-4-5`

## Notes
- Transcription uses whisper.cpp; you can choose a model size later (tiny/base/small).
- Summarization uploads ONLY the transcript text (not the audio).
