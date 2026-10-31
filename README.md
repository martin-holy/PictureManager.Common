# PictureManager.Common

Shared application logic and data layer for [Picture Manager for Windows (WPF)](https://github.com/martin-holy/PictureManager) and [Picture Manager for Android](https://github.com/martin-holy/PictureManager.Android).

`PictureManager.Common` contains the functionality shared by the platform-specific applications. It keeps media management, metadata handling, domain models, persistence and most application workflows in one place, while leaving platform-dependent operations and UI integration to the respective frontend projects.

This is an application-specific library, not a standalone media-management framework or a general-purpose collection of utilities.

## What's included

### Media management

- Common image, video and media-item models, repositories and services.
- Media import, metadata reading and synchronization with the application's data model.
- Media-item filtering, sorting, grouping, selection and collection-view logic.
- Image and video metadata editing, including ratings, comments, keywords, people and location information.
- Thumbnail and cache coordination through platform-provided services.
- Image resizing, compression, rotation and image-to-video workflows.
- Image comparison and perceptual-hash based duplicate detection.

### Organization

- Folder trees, favorite folders and folder keywords.
- Hierarchical keywords and people, including groups and relationships to media items.
- GeoNames and geographic locations.
- Ratings and configurable viewers.
- Video clips and image segments, including their people and keyword associations.

### Application workflows

- Browser, media viewer and slideshow view models.
- Segment and video-clip management, export and related workflows.
- Settings and application configuration.
- File-operation workflows and collision handling.
- Logging, progress reporting, dialogs and application commands.
- Plugin discovery and lifecycle integration.

## Architecture

The shared layer is organized around three main parts:

- **Models and repositories** – represent application data and manage relationships between folders, media items, people, keywords, locations, segments and video clips.
- **Services and application state** – coordinate importing, metadata operations, filtering, editing workflows and interactions between features.
- **View models and UI composition** – expose application state and commands through the cross-platform controls and abstractions provided by [MH.UI](https://github.com/martin-holy/MH.UI).

The `Core` class coordinates initialization and exposes the shared application services:

- `CoreR` – repositories, data sources and the `SimpleDB` database.
- `CoreS` – shared application state and feature services.
- `CoreVM` – application view models, commands and UI composition.
- `ICoreP` – platform services supplied by the frontend.

Platform-specific functionality is kept behind interfaces and platform-provided services. For example, the frontend supplies image-thumbnail creation, cache paths and media-player instances. This allows the common application logic to be reused without depending directly on WPF or Android UI implementations.

## Persistence

Application data is managed by `MH.Utils.SimpleDB`, using repository and relation data sources. The common layer defines the data models, repositories, relationships and CSV-backed persistence sources. Database initialization includes loading, linking references and properties, and applying database migrations.

User settings are stored separately in JSON and use source-generated `System.Text.Json` serialization metadata.

## Dependencies

- [MH.UI](https://github.com/martin-holy/MH.UI) – cross-platform UI controls, view models, dialogs and interaction abstractions.
- [MH.Utils](https://github.com/martin-holy/MH.Utils) – collections, commands, data structures, persistence and imaging/metadata functionality.

The project targets **.NET 8** and does not reference WPF. Platform-specific frontends reference this project and provide the services required by the shared layer.

## Related repositories

- [Picture Manager (WPF)](https://github.com/martin-holy/PictureManager) – Windows desktop application.
- [Picture Manager (Android)](https://github.com/martin-holy/PictureManager.Android) – Android application.
- [MH.UI](https://github.com/martin-holy/MH.UI) – cross-platform UI framework.
- [MH.UI.WPF](https://github.com/martin-holy/MH.UI.WPF) – WPF integration for MH.UI.
- [MH.Utils](https://github.com/martin-holy/MH.Utils) – shared .NET utilities and infrastructure.
