# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-01-XX

### Added
- Initial release of UI Canvas Isolation tool
- Automatic UI element detection and canvas isolation
- Scene View overlay toggle for enabling/disabling the tool
- Support for Screen Space Overlay canvases
- Configurable settings for different canvas types
- Component caching for performance optimization
- Debouncing to prevent excessive operations
- Undo support for all operations
- Multi-scene view support
- Error handling and safe scene view operations
- Memory management and cache cleanup
- Comprehensive documentation and samples

### Features
- **Smart Canvas Detection**: Finds the nearest canvas in the hierarchy
- **Automatic 2D Mode**: Switches to orthographic mode for UI editing
- **Layer Isolation**: Shows only UI layer when editing UI elements
- **Context Framing**: Frames the nearest canvas to show UI elements in context
- **State Restoration**: Properly restores original view when selecting non-UI objects
- **Performance Optimized**: Component caching and debouncing
- **Configurable**: Support for different canvas render modes
- **Safe Operations**: Error handling and undo support

### Technical Details
- Unity 2022.3.0f1+ compatibility
- No external dependencies
- Editor-only tool
- MIT License

