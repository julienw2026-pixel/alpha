# whisper.cpp integration plan

We will download prebuilt Windows binaries in GitHub Actions and package them with the app.

- Download: whisper.cpp releases include `whisper-bin-x64.zip` (or similar)
- Provide `whisper-cli.exe`
- Download model: ggml-small.bin (or base)

The app calls:
`whisper-cli.exe -m <model> -f <wav> -otxt -of <outBase> --language auto`

Then read `<outBase>.txt`.
