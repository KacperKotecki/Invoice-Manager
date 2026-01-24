![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-purple)
![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC%205-blue)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-6-red)
![Bootstrap](https://img.shields.io/badge/Frontend-Bootstrap%205-7952b3)
![License](https://img.shields.io/badge/License-MIT-green)

> **A professional B2B system for managing invoices, cash flow, and contractors. Designed to comply with Polish accounting standards.**

---

## 📺 Demo (Preview)
<img width="1292" height="887" alt="index" src="https://github.com/user-attachments/assets/c0cfd650-9788-4fed-bd30-bed0bc120c82" />
<img width="1315" height="1053" alt="main" src="https://github.com/user-attachments/assets/170d5a97-8d4b-4f60-9021-16cbd9ae4ddf" />

<img width="1291" height="1092" alt="create" src="https://github.com/user-attachments/assets/427745fc-3e96-4d59-8bcb-6804ef215125" />

## 🚀 About the Project

Invoice Manager is more than just a document generator. It solves real **Cash Flow management** problems for small service businesses (B2B).

The application addresses real-world business challenges:
* **Data Integrity:** What happens to an old invoice if the client changes their address? (Solution: **Snapshotting**).
* **Financial Liquidity:** Who is late with payments? How much was paid in advance? (Solution: **KPI Dashboard** and **Split Payments**).
* **Legal Compliance:** Does the invoice meet official requirements? (Solution: **Validations** and **Humanizer**).

---

## 🛠️ Key Features

### 1. 🛡️ Data Integrity & Snapshotting
I implemented the **Snapshot** pattern to "freeze" client data when an invoice is created.
* Updates to the `Clients` table **do not affect** historical invoices.
* This guarantees data immutability for accounting purposes.

### 2. 💰 Payment Handling (Partial Payments)
The system supports **partial payments** and down payments.
* Automatically calculates the `Remaining Amount`.
* Real-time status updates: `Draft` → `Pending` → `Paid`.
* Business logic prevents editing of invoices that have been paid or sent.

### 3. 📄 PDF Generator & Compliance
Generates legally binding PDF documents using the **Rotativa** library.
* Automatic "number to words" conversion (**Humanizer**).
* Dynamic mapping of VAT rates.
* Supports various units (`hours`, `km`, `pcs`, `set`).

### 4. 📊 Analytical Dashboard
A command center for the business owner:
* **KPIs:** Real-time revenue from the last 30 days.
* **Alerts:** Overdue invoices highlighted in red.
* **Quick Actions:** One-click payment registration.

---

## 💻 Tech Stack

**Backend:**
* C# / .NET Framework
* ASP.NET MVC 5
* Entity Framework 6 (Code First)
* LINQ

**Database:**
* MS SQL Server
* Repository Pattern (Separation of logic from data access)

**Frontend:**
* Razor Views (`.cshtml`)
* Bootstrap 5 (Dark Mode Support)
* jQuery / AJAX (Modal handling)
* FontAwesome

**Tools:**
* **Rotativa:** Generating PDFs from HTML views.
* **Humanizer:** Logic for converting numbers to text.
* **Git:** Version control.

---

## ⚙️ Installation & Setup

1.  Clone the repository:
    ```bash
    git clone [https://github.com/TwojNick/InvoiceManager.git](https://github.com/TwojNick/InvoiceManager.git)
    ```
2.  Open the `.sln` file in Visual Studio 2022.
3.  Update the `connectionString` in `Web.config` to match your local SQL Server.
4.  Run `Update-Database` in the **Package Manager Console**:
    ```powershell
    Update-Database
    ```
5.  Run the project (F5).

---

## 🔮 Roadmap

* [ ] Add unit tests (NUnit/xUnit).
* [ ] Migrate to .NET 8 / ASP.NET Core.
* [ ] Integrate with GUS API (fetch company data by Tax ID).
* [ ] Email sending (SMTP).

---

## 📬 Contact

**Kacper Kotecki** 📧 kacperkotecki@protonmail.com
🔗 [LinkedIn Profile](https://www.linkedin.com/in/kacper-kotecki-349829295/)
