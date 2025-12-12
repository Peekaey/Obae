#!/bin/bash

# =============================================================================
# Obae - macOS arm64 App Bundle Build Script
# =============================================================================
# This script builds the Obae Avalonia UI application as a macOS .app bundle
# for arm64 (Apple Silicon) with an embedded blank SQLite database.
# =============================================================================

# Disclaimer - AI Generated

set -e  # Exit on error

# Configuration
APP_NAME="Obae"
BUNDLE_NAME="Obae.app"
PROJECT_PATH="../Obae/Obae.csproj"
OUTPUT_DIR="./dist/Obae-osx-arm64-bundle"
RUNTIME_IDENTIFIER="osx-arm64"
CONFIGURATION="Release"
DATABASE_NAME="obae-app.db"
BUNDLE_IDENTIFIER="com.obae.app"
VERSION="1.0.0"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=============================================${NC}"
echo -e "${BLUE}  Obae - macOS arm64 App Bundle Builder${NC}"
echo -e "${BLUE}=============================================${NC}"
echo ""

# Step 1: Clean previous builds
echo -e "${YELLOW}[1/8] Cleaning previous builds...${NC}"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Step 2: Restore NuGet packages
echo -e "${YELLOW}[2/8] Restoring NuGet packages...${NC}"
dotnet restore "$PROJECT_PATH" --runtime "$RUNTIME_IDENTIFIER"

# Step 3: Build and publish the application
echo -e "${YELLOW}[3/8] Building and publishing application...${NC}"
PUBLISH_DIR="$OUTPUT_DIR/publish"
dotnet publish "$PROJECT_PATH" \
    --configuration "$CONFIGURATION" \
    --runtime "$RUNTIME_IDENTIFIER" \
    --self-contained true \
    --output "$PUBLISH_DIR" \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=false \
    -p:PublishReadyToRun=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:IncludeAllContentForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true

# Step 4: Create .app bundle structure
echo -e "${YELLOW}[4/8] Creating .app bundle structure...${NC}"
BUNDLE_PATH="$OUTPUT_DIR/$BUNDLE_NAME"
CONTENTS_PATH="$BUNDLE_PATH/Contents"
MACOS_PATH="$CONTENTS_PATH/MacOS"
RESOURCES_PATH="$CONTENTS_PATH/Resources"

mkdir -p "$MACOS_PATH"
mkdir -p "$RESOURCES_PATH"

# Step 5: Copy executable to bundle
echo -e "${YELLOW}[5/8] Copying executable to bundle...${NC}"
cp "$PUBLISH_DIR/$APP_NAME" "$MACOS_PATH/"
chmod +x "$MACOS_PATH/$APP_NAME"

# Copy any additional files that might be needed
if [ -f "$PUBLISH_DIR/playwright.ps1" ]; then
    cp "$PUBLISH_DIR/playwright.ps1" "$RESOURCES_PATH/"
fi

# Handle app icon - supports .icns, .png, .jpg, .jpeg
# Applies macOS Big Sur style rounded corners using native Core Graphics
echo -e "${YELLOW}[5.5/8] Processing app icon...${NC}"
ICON_ICNS="../Obae/Assets/AppIcon.icns"
ICON_PNG="../Obae/Assets/AppIcon.png"
ICON_JPG="../Obae/Assets/AppIcon.jpg"
ICON_JPEG="../Obae/Assets/AppIcon.jpeg"

if [ -f "$ICON_ICNS" ]; then
    # Use existing .icns file directly
    cp "$ICON_ICNS" "$RESOURCES_PATH/AppIcon.icns"
    echo -e "${GREEN}  ✓ App icon copied (icns)${NC}"
elif [ -f "$ICON_PNG" ] || [ -f "$ICON_JPG" ] || [ -f "$ICON_JPEG" ]; then
    # Determine which image file to use
    if [ -f "$ICON_PNG" ]; then
        SOURCE_ICON="$ICON_PNG"
    elif [ -f "$ICON_JPG" ]; then
        SOURCE_ICON="$ICON_JPG"
    else
        SOURCE_ICON="$ICON_JPEG"
    fi
    
    echo -e "${BLUE}  Converting $SOURCE_ICON to .icns...${NC}"
    
    # Create temporary directory for processing
    TEMP_DIR=$(mktemp -d)
    ICONSET_DIR="$TEMP_DIR/AppIcon.iconset"
    mkdir -p "$ICONSET_DIR"
    
    # Get image dimensions
    IMG_WIDTH=$(sips -g pixelWidth "$SOURCE_ICON" | tail -1 | awk '{print $2}')
    IMG_HEIGHT=$(sips -g pixelHeight "$SOURCE_ICON" | tail -1 | awk '{print $2}')
    
    # Convert to proper PNG format and make it square if needed
    SQUARE_ICON="$TEMP_DIR/square_icon.png"
    
    if [ "$IMG_WIDTH" -ne "$IMG_HEIGHT" ]; then
        echo -e "${BLUE}  Image is not square (${IMG_WIDTH}x${IMG_HEIGHT}), making it square...${NC}"
        # Use the larger dimension to create a square
        if [ "$IMG_WIDTH" -gt "$IMG_HEIGHT" ]; then
            MAX_DIM=$IMG_WIDTH
        else
            MAX_DIM=$IMG_HEIGHT
        fi
        # Resize to fit in square while maintaining aspect ratio, then pad
        sips -s format png --resampleHeightWidthMax $MAX_DIM "$SOURCE_ICON" --out "$TEMP_DIR/resized.png" > /dev/null 2>&1
        # Pad to make square (sips -p pads the image)
        sips -p $MAX_DIM $MAX_DIM "$TEMP_DIR/resized.png" --out "$SQUARE_ICON" > /dev/null 2>&1
    else
        # Already square, just convert to PNG format
        sips -s format png "$SOURCE_ICON" --out "$SQUARE_ICON" > /dev/null 2>&1
    fi
    
    # Apply rounded corners using native Swift/Core Graphics (no external dependencies)
    # macOS Big Sur uses ~22.37% corner radius for app icons
    FINAL_ICON="$TEMP_DIR/rounded_icon.png"
    SWIFT_SCRIPT="$TEMP_DIR/round_corners.swift"
    
    cat > "$SWIFT_SCRIPT" << 'SWIFT_EOF'
import Cocoa

let args = CommandLine.arguments
guard args.count == 3 else {
    print("Usage: round_corners <input.png> <output.png>")
    exit(1)
}

let inputPath = args[1]
let outputPath = args[2]

guard let image = NSImage(contentsOfFile: inputPath) else {
    print("Error: Cannot load image from \(inputPath)")
    exit(1)
}

let originalSize = image.size
let canvasSize = Int(max(originalSize.width, originalSize.height))

// Scale down to ~80% to match other macOS app icons
let scale: CGFloat = 0.80
let artworkSize = CGFloat(canvasSize) * scale
let padding = (CGFloat(canvasSize) - artworkSize) / 2.0

// The artwork rect (centered with padding)
let artworkRect = NSRect(
    x: padding,
    y: padding,
    width: artworkSize,
    height: artworkSize
)

// Subtle rounded corners (12.5% of the artwork size)
let cornerRadius = artworkSize * 0.125

// Create bitmap with alpha channel
let bitmapRep = NSBitmapImageRep(
    bitmapDataPlanes: nil,
    pixelsWide: canvasSize,
    pixelsHigh: canvasSize,
    bitsPerSample: 8,
    samplesPerPixel: 4,
    hasAlpha: true,
    isPlanar: false,
    colorSpaceName: .deviceRGB,
    bytesPerRow: 0,
    bitsPerPixel: 0
)!

NSGraphicsContext.saveGraphicsState()
NSGraphicsContext.current = NSGraphicsContext(bitmapImageRep: bitmapRep)

// Clear to transparent
NSColor.clear.set()
NSRect(x: 0, y: 0, width: canvasSize, height: canvasSize).fill()

// Create rounded rect path for the artwork area
let path = NSBezierPath(roundedRect: artworkRect, xRadius: cornerRadius, yRadius: cornerRadius)
path.addClip()

// Draw the original image scaled into the artwork rect
image.draw(in: artworkRect, from: NSRect(origin: .zero, size: originalSize), operation: .sourceOver, fraction: 1.0)

NSGraphicsContext.restoreGraphicsState()

// Save as PNG
guard let pngData = bitmapRep.representation(using: .png, properties: [:]) else {
    print("Error: Cannot create PNG data")
    exit(1)
}

do {
    try pngData.write(to: URL(fileURLWithPath: outputPath))
} catch {
    print("Error: Cannot write to \(outputPath): \(error)")
    exit(1)
}
SWIFT_EOF

    echo -e "${BLUE}  Applying rounded corners (macOS Big Sur style)...${NC}"
    if swiftc -o "$TEMP_DIR/round_corners" "$SWIFT_SCRIPT" 2>/dev/null && \
       "$TEMP_DIR/round_corners" "$SQUARE_ICON" "$FINAL_ICON" 2>/dev/null; then
        echo -e "${GREEN}  ✓ Rounded corners applied${NC}"
    else
        echo -e "${YELLOW}  ⚠ Could not apply rounded corners, using square icon${NC}"
        FINAL_ICON="$SQUARE_ICON"
    fi
    
    # Generate all required sizes using sips (built into macOS)
    sips -z 16 16     "$FINAL_ICON" --out "$ICONSET_DIR/icon_16x16.png" > /dev/null 2>&1
    sips -z 32 32     "$FINAL_ICON" --out "$ICONSET_DIR/icon_16x16@2x.png" > /dev/null 2>&1
    sips -z 32 32     "$FINAL_ICON" --out "$ICONSET_DIR/icon_32x32.png" > /dev/null 2>&1
    sips -z 64 64     "$FINAL_ICON" --out "$ICONSET_DIR/icon_32x32@2x.png" > /dev/null 2>&1
    sips -z 128 128   "$FINAL_ICON" --out "$ICONSET_DIR/icon_128x128.png" > /dev/null 2>&1
    sips -z 256 256   "$FINAL_ICON" --out "$ICONSET_DIR/icon_128x128@2x.png" > /dev/null 2>&1
    sips -z 256 256   "$FINAL_ICON" --out "$ICONSET_DIR/icon_256x256.png" > /dev/null 2>&1
    sips -z 512 512   "$FINAL_ICON" --out "$ICONSET_DIR/icon_256x256@2x.png" > /dev/null 2>&1
    sips -z 512 512   "$FINAL_ICON" --out "$ICONSET_DIR/icon_512x512.png" > /dev/null 2>&1
    sips -z 1024 1024 "$FINAL_ICON" --out "$ICONSET_DIR/icon_512x512@2x.png" > /dev/null 2>&1
    
    # Convert iconset to icns using iconutil (built into macOS)
    if iconutil -c icns "$ICONSET_DIR" -o "$RESOURCES_PATH/AppIcon.icns" 2>/dev/null; then
        echo -e "${GREEN}  ✓ App icon created from $(basename "$SOURCE_ICON")${NC}"
    else
        echo -e "${RED}  ✗ Failed to create icon. Please provide a square PNG image (e.g., 1024x1024)${NC}"
    fi
    
    # Clean up temp directory
    rm -rf "$TEMP_DIR"
else
    echo -e "${YELLOW}  ⚠ No app icon found - using default macOS icon${NC}"
    echo -e "${BLUE}    To add an icon, place one of these in Obae/Assets/:${NC}"
    echo -e "${BLUE}      - AppIcon.icns (macOS icon)${NC}"
    echo -e "${BLUE}      - AppIcon.png (square image, ideally 1024x1024)${NC}"
    echo -e "${BLUE}      - AppIcon.jpg/jpeg (square image, ideally 1024x1024)${NC}"
fi

# Step 6: Create blank SQLite database in Resources
echo -e "${YELLOW}[6/8] Creating blank SQLite database...${NC}"
DATABASE_PATH="$RESOURCES_PATH/$DATABASE_NAME"

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
    echo -e "${GREEN}  ✓ Created blank database (minimal file)${NC}"
fi

# Step 7: Create Info.plist
echo -e "${YELLOW}[7/8] Creating Info.plist...${NC}"
cat > "$CONTENTS_PATH/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>${APP_NAME}</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>CFBundleIdentifier</key>
    <string>${BUNDLE_IDENTIFIER}</string>
    <key>CFBundleName</key>
    <string>${APP_NAME}</string>
    <key>CFBundleDisplayName</key>
    <string>${APP_NAME}</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright © 2024 Obae. All rights reserved.</string>
    <key>LSArchitecturePriority</key>
    <array>
        <string>arm64</string>
    </array>
    <key>NSRequiresAquaSystemAppearance</key>
    <false/>
</dict>
</plist>
EOF

# Step 8: Create launcher script (handles database location without changing working directory)
echo -e "${YELLOW}[8/8] Creating launcher script...${NC}"

# Rename the actual executable
mv "$MACOS_PATH/$APP_NAME" "$MACOS_PATH/${APP_NAME}-bin"

# Create launcher script that sets up the database path via environment variable
# IMPORTANT: Do NOT change working directory - this breaks native library loading
cat > "$MACOS_PATH/$APP_NAME" << 'LAUNCHER'
#!/bin/bash

# Get the directory where the app bundle is located
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
RESOURCES_DIR="$SCRIPT_DIR/../Resources"

# Set up application data directory
APP_SUPPORT_DIR="$HOME/Library/Application Support/Obae"
mkdir -p "$APP_SUPPORT_DIR"

# Copy database if it doesn't exist
DATABASE_NAME="obae-app.db"
if [ ! -f "$APP_SUPPORT_DIR/$DATABASE_NAME" ]; then
    if [ -f "$RESOURCES_DIR/$DATABASE_NAME" ]; then
        cp "$RESOURCES_DIR/$DATABASE_NAME" "$APP_SUPPORT_DIR/"
        echo "Copied initial database to $APP_SUPPORT_DIR"
    fi
fi

# Export the database path as an environment variable
# The application should read this to locate the database
export OBAE_DB_PATH="$APP_SUPPORT_DIR/$DATABASE_NAME"
export OBAE_APP_SUPPORT_DIR="$APP_SUPPORT_DIR"

# Launch the actual application from its directory
# DO NOT change working directory - native libraries are extracted relative to the executable
exec "$SCRIPT_DIR/Obae-bin" "$@"
LAUNCHER

chmod +x "$MACOS_PATH/$APP_NAME"

# Clean up publish directory
rm -rf "$PUBLISH_DIR"

# Summary
echo ""
echo -e "${GREEN}=============================================${NC}"
echo -e "${GREEN}  Build Complete!${NC}"
echo -e "${GREEN}=============================================${NC}"
echo ""
echo -e "${GREEN}App Bundle: ${NC}$BUNDLE_PATH"
echo ""
echo -e "${GREEN}Bundle contents:${NC}"
find "$BUNDLE_PATH" -type f | head -20
echo ""
echo -e "${BLUE}To install:${NC}"
echo -e "  Drag $BUNDLE_NAME to /Applications"
echo ""
echo -e "${BLUE}To run directly:${NC}"
echo -e "  open $BUNDLE_PATH"
echo ""
echo -e "${YELLOW}Notes:${NC}"
echo -e "  • The database will be stored in ~/Library/Application Support/Obae/"
echo -e "  • On first run, allow the app in System Preferences > Security & Privacy"
echo -e "  • Playwright browsers may need to be installed on first use"
echo ""

# Create a DMG (optional)
read -p "Create DMG installer? (y/n): " CREATE_DMG
if [[ "$CREATE_DMG" =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Creating DMG installer...${NC}"
    DMG_PATH="$OUTPUT_DIR/Obae-arm64.dmg"
    
    # Create a temporary directory for DMG contents
    DMG_TEMP="$OUTPUT_DIR/dmg-temp"
    mkdir -p "$DMG_TEMP"
    cp -R "$BUNDLE_PATH" "$DMG_TEMP/"
    
    # Create symbolic link to Applications
    ln -s /Applications "$DMG_TEMP/Applications"
    
    # Create DMG
    hdiutil create -volname "Obae" -srcfolder "$DMG_TEMP" -ov -format UDZO "$DMG_PATH"
    
    # Clean up
    rm -rf "$DMG_TEMP"
    
    echo -e "${GREEN}DMG created: ${NC}$DMG_PATH"
fi

echo -e "${GREEN}Done!${NC}"
