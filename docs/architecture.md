# SOSWIM Architecture

## 1. Clean Layered Architecture (ASP.NET Core 8)

The project strictly follows layered clean architecture separation:

```mermaid
flowchart TD
    subgraph UI ["Presentation Layer (ASP.NET Core 8 MVC)"]
        V1["Views/Home/Index.cshtml (Landing & App Showcase)"]
        V2["Views/Tracking/Index.cshtml (Telemetry Dashboard)"]
        C1["Controllers/HomeController.cs"]
        C2["Controllers/TrackingController.cs"]
    end

    subgraph ServiceLayer ["Application & Domain Services"]
        S1["ISwimmerTrackingService"]
        S2["SwimmerTrackingService (In-Memory Concurrent State Store)"]
    end

    subgraph DomainModels ["Models & DTOs"]
        M1["SwimmerSession"]
        M2["SwimmerVitals"]
        M3["SwimmerStatus (Enum)"]
        VM["TrackingViewModels & DTOs"]
    end

    subgraph TestingSuite ["Automated Testing"]
        T1["Web_SoSwim.Tests (xUnit)"]
    end

    V1 --> C1
    V2 --> C2
    C1 --> S1
    C2 --> S1
    S1 --> S2
    S2 --> DomainModels
    T1 -. Tests .-> S1
```

## 2. Technology Stack & Key Highlights
- **Framework**: .NET 8.0 (C# 12).
- **Architecture**: MVC with decoupled Service Layer.
- **State Management**: Concurrent In-Memory Session Cache (`ConcurrentDictionary`), structured so it is 100% plug-and-play ready for Entity Framework Core / Supabase PostgreSQL migrations in subsequent stages.
- **Testing**: xUnit with 100% pass rate on business rules (especially the 50% risk threshold gatekeeper).
- **Design Craftsmanship**:
  - Pure vector Lucide SVGs (Zero emoji clutter).
  - High-contrast marine navy & electric cyan palette (`#050b14`, `#00e5ff`, `#0284c7`).
  - Native RTL layout with Tajawal typography for optimal Arabic user experience.
