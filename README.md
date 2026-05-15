
# AmarTools

AmarTools is an enterprise-grade, multi-tenant modular web application built using the .NET ecosystem. The platform encapsulates multiple utility tools into isolated feature modules, leveraging a strict separation of concerns, data isolation via multi-tenancy filters, and automated asset generation workflows.

---

## 🛠️ Core Tools & Modules

The application partitions its business logic into distinct, decoupled modules located under `src/Modules`:

*   **Dashboard**: High-level overview metrics, analytics, and tenant-specific data visualization.
*   **BannerGenerator**: Dynamic image processing engine designed to generate custom promotional banners and media.
*   **Voting**: A secure, tenant-isolated polling and digital voting management system.
*   **CertificateGenerator**: Automated engine that utilizes external PDF services to dynamically render and issue certificates.
*   **GuestCheckIn**: Event management tool facilitating real-time guest tracking and verification.
*   **ChequePrinting**: A specialized financial utility featuring localized cheque rendering via a hardware printing simulation service.

---

## 🚀 Run & Deployment Details

### Prerequisites
*   [.NET SDK](https://dotnet.microsoft.com/download)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop) (Required to host backend database and infrastructure containers)



This project was designed, developed, and documented as a collaborative effort by:

*   **[Inzamam Islam](https://github.com/mishuk-11)**
*   **[Binoy Dev](https://github.com/Devzit0123)**
*   **[Shaibal Dey](https://github.com/Shaibal-2003)**
*   **[Ummay Shoyeba Cherry](https://github.com/cherry03-debug)**
*   **[Nure Muskan](https://github.com/soaibamuskan002)**
