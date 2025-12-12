#!/bin/bash

# =============================================================================
# Obae - macOS arm64 Build Script
# =============================================================================
# This script builds the Obae Avalonia UI application as a single executable
# for macOS arm64 (Apple Silicon) and packages it with a blank SQLite database.
# =============================================================================

# Disclaimer - AI Generated

set -e  # Exit on error

# Configuration
APP_NAME="Obae"
PROJECT_PATH="../Obae/Obae.csproj"
OUTPUT_DIR="./dist/Obae-osx-arm64"
RUNTIME_IDENTIFIER="osx-arm64"
CONFIGURATION="Release"
DATABASE_NAME="obae-app.db"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=============================================${NC}"
echo -e "${BLUE}  Obae - macOS arm64 Build Script${NC}"
echo -e "${BLUE}=============================================${NC}"
echo ""

# Step 1: Clean previous builds
echo -e "${YELLOW}[1/6] Cleaning previous builds...${NC}"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Step 2: Restore NuGet packages
echo -e "${YELLOW}[2/6] Restoring NuGet packages...${NC}"
dotnet restore "$PROJECT_PATH" --runtime "$RUNTIME_IDENTIFIER"

# Step 3: Build and publish the application
echo -e "${YELLOW}[3/6] Building and publishing application...${NC}"
dotnet publish "$PROJECT_PATH" \
    --configuration "$CONFIGURATION" \
    --runtime "$RUNTIME_IDENTIFIER" \
    --self-contained true \
    --output "$OUTPUT_DIR" \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=false \
    -p:PublishReadyToRun=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:IncludeAllContentForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true

# Step 4: Create blank SQLite database
echo -e "${YELLOW}[4/6] Creating blank SQLite database...${NC}"
DATABASE_PATH="$OUTPUT_DIR/$DATABASE_NAME"

# Remove any existing database file
rm -f "$DATABASE_PATH"

# Create a blank SQLite database file
# Using sqlite3 to create an empty database with proper SQLite format
if command -v sqlite3 &> /dev/null; then
    # Create a blank database by creating and dropping a dummy table
    # This forces SQLite to write a valid database file to disk
    sqlite3 "$DATABASE_PATH" "CREATE TABLE _init (id INTEGER); DROP TABLE _init; VACUUM;"
    echo -e "${GREEN}  ✓ Created blank database using sqlite3${NC}"
else
    # Fallback: Create a minimal valid SQLite database file
    # SQLite3 file format header (first 16 bytes)
    printf 'SQLite format 3\0' > "$DATABASE_PATH"
    # Page size (2 bytes at offset 16) - set to 4096
    printf '\x10\x00' >> "$DATABASE_PATH"
    # Write and read versions (1 byte each at offset 18 and 19)
    printf '\x01\x01' >> "$DATABASE_PATH"
    # Reserved bytes, then padding to page size
    dd if=/dev/zero bs=1 count=4076 >> "$DATABASE_PATH" 2>/dev/null
    echo -e "${GREEN}  ✓ Created blank database (sqlite3 not found, created minimal file)${NC}"
fi

# Step 5: Install Playwright browsers (optional - check if needed)
echo -e "${YELLOW}[5/6] Checking Playwright setup...${NC}"
PLAYWRIGHT_SCRIPT="$OUTPUT_DIR/playwright.ps1"
if [ -f "$PLAYWRIGHT_SCRIPT" ]; then
    echo -e "${BLUE}  ℹ Playwright script found at: $PLAYWRIGHT_SCRIPT${NC}"
    echo -e "${BLUE}  ℹ Users may need to run: pwsh playwright.ps1 install${NC}"
else
    echo -e "${BLUE}  ℹ Playwright browsers will be installed on first run${NC}"
fi

# Step 6: Set executable permissions
echo -e "${YELLOW}[6/6] Setting executable permissions...${NC}"
chmod +x "$OUTPUT_DIR/$APP_NAME"

# Summary
echo ""
echo -e "${GREEN}=============================================${NC}"
echo -e "${GREEN}  Build Complete!${NC}"
echo -e "${GREEN}=============================================${NC}"
echo ""
echo -e "${GREEN}Output directory: ${NC}$OUTPUT_DIR"
echo ""
echo -e "${GREEN}Contents:${NC}"
ls -lh "$OUTPUT_DIR"
echo ""
echo -e "${BLUE}To run the application:${NC}"
echo -e "  cd $OUTPUT_DIR"
echo -e "  ./$APP_NAME"
echo ""
echo -e "${YELLOW}Note: On first run, you may need to:${NC}"
echo -e "  1. Allow the app in System Preferences > Security & Privacy"
echo -e "  2. Install Playwright browsers if needed"
echo ""
