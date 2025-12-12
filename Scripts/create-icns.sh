#!/bin/bash

# =============================================================================
# Create macOS .icns icon from PNG
# =============================================================================
# Usage: ./create-icns.sh <input.png> [output.icns]
# The input PNG should ideally be 1024x1024 pixels
# =============================================================================

# Disclaimer - AI Generated

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Check arguments
if [ -z "$1" ]; then
    echo -e "${RED}Error: No input PNG specified${NC}"
    echo "Usage: $0 <input.png> [output.icns]"
    echo ""
    echo "Example: $0 my-icon.png ../Obae/Assets/AppIcon.icns"
    exit 1
fi

INPUT_PNG="$1"
OUTPUT_ICNS="${2:-AppIcon.icns}"

# Check if input file exists
if [ ! -f "$INPUT_PNG" ]; then
    echo -e "${RED}Error: Input file '$INPUT_PNG' not found${NC}"
    exit 1
fi

# Check if it's a PNG
if ! file "$INPUT_PNG" | grep -q "PNG"; then
    echo -e "${YELLOW}Warning: Input file may not be a PNG image${NC}"
fi

echo -e "${BLUE}Creating macOS icon from: ${NC}$INPUT_PNG"

# Create temporary iconset directory
ICONSET_DIR=$(mktemp -d)/AppIcon.iconset
mkdir -p "$ICONSET_DIR"

echo -e "${YELLOW}Generating icon sizes...${NC}"

# Generate all required sizes
sips -z 16 16     "$INPUT_PNG" --out "$ICONSET_DIR/icon_16x16.png" > /dev/null 2>&1
sips -z 32 32     "$INPUT_PNG" --out "$ICONSET_DIR/icon_16x16@2x.png" > /dev/null 2>&1
sips -z 32 32     "$INPUT_PNG" --out "$ICONSET_DIR/icon_32x32.png" > /dev/null 2>&1
sips -z 64 64     "$INPUT_PNG" --out "$ICONSET_DIR/icon_32x32@2x.png" > /dev/null 2>&1
sips -z 128 128   "$INPUT_PNG" --out "$ICONSET_DIR/icon_128x128.png" > /dev/null 2>&1
sips -z 256 256   "$INPUT_PNG" --out "$ICONSET_DIR/icon_128x128@2x.png" > /dev/null 2>&1
sips -z 256 256   "$INPUT_PNG" --out "$ICONSET_DIR/icon_256x256.png" > /dev/null 2>&1
sips -z 512 512   "$INPUT_PNG" --out "$ICONSET_DIR/icon_256x256@2x.png" > /dev/null 2>&1
sips -z 512 512   "$INPUT_PNG" --out "$ICONSET_DIR/icon_512x512.png" > /dev/null 2>&1
sips -z 1024 1024 "$INPUT_PNG" --out "$ICONSET_DIR/icon_512x512@2x.png" > /dev/null 2>&1

echo -e "${YELLOW}Converting to .icns...${NC}"

# Convert iconset to icns
iconutil -c icns "$ICONSET_DIR" -o "$OUTPUT_ICNS"

# Clean up
rm -rf "$(dirname "$ICONSET_DIR")"

echo -e "${GREEN}✓ Icon created: ${NC}$OUTPUT_ICNS"
echo ""
echo -e "${BLUE}To use this icon:${NC}"
echo -e "  1. Copy it to: Obae/Assets/AppIcon.icns"
echo -e "  2. Rebuild the app with: ./build-macos-arm64-bundle.sh"
