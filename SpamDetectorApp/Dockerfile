# ─────────────────────────────────────────────────────────────────────────────
# Dockerfile — SMS Spam Detector AI (Windows Forms)
# ─────────────────────────────────────────────────────────────────────────────

# Use Windows Server Core as base image
FROM mcr.microsoft.com/windows/servercore:ltsc2022

# Set working directory (absolute path, properly escaped)
WORKDIR C:\\app

# Copy published files (use proper backslashes and no trailing dot issues)
COPY bin\\Release\\net8.0-windows\\win-x64\\publish\\ .

# Labels for documentation
LABEL maintainer="Taimoor Arshad"
LABEL description="SMS Spam Detector AI — ML.NET Windows Forms App"
LABEL version="1.0"

# Set the entrypoint to the Windows Forms executable
ENTRYPOINT ["SpamDetectorApp.exe"]