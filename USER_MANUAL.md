# AP Staging Module — User Manual

## Table of Contents

1. [Getting Started](#1-getting-started)
2. [Initial Setup — Preferences](#2-initial-setup--preferences)
3. [Viewing Staged Invoices (List View)](#3-viewing-staged-invoices-list-view)
4. [Reviewing a Staged Invoice](#4-reviewing-a-staged-invoice)
5. [Creating an AP Bill](#5-creating-an-ap-bill)
6. [Creating a Staging Record Manually](#6-creating-a-staging-record-manually)
7. [Field Reference](#7-field-reference)
8. [Common Scenarios](#8-common-scenarios)

---

## 1. Getting Started

The AP Staging module sits between your external e-invoicing provider (e.g. Storecove) and Acumatica's standard Accounts Payable module. Invoices arrive automatically via webhook and are held in staging until a user reviews and approves them.

**Before using the module you must complete the one-time setup in Section 2.**

To navigate to the module:

1. Log in to Acumatica.
2. In the left menu, go to **Accounts Payable**.
3. You will find:
   - **AP Staging Form** — the list of all staged invoices (GI3010IL)
   - Under **Preferences** — **AP Staging Preferences** (AP301092)

---

## 2. Initial Setup — Preferences

> **Who does this:** System administrator or implementation consultant, once only.

Navigate to **Accounts Payable > Preferences > AP Staging Preferences** (AP301092).

This screen stores the API credentials used when the system creates AP Bills on your behalf.

### 2.1 Acumatica API Connection

These fields tell the module how to connect back to Acumatica's REST API.

| Field | What to enter | Example |
|-------|--------------|---------|
| **Acumatica Base URL** | The root URL of your Acumatica site, no trailing slash | `http://localhost/saga` |
| **Client ID** | The OAuth2 client ID created in Acumatica (SM201060) | `MyAPStagingApp` |
| **Client Secret** | The client secret for that OAuth2 app (stored encrypted) | `••••••••` |
| **Username** | An Acumatica user account that has AP Bill creation rights | `apstaging.svc` |
| **Password** | That user's password (stored encrypted) | `••••••••` |
| **Scope** | OAuth2 scope — leave blank to default to `api` | `api` |
| **Entity Endpoint (APStaging)** | REST path to the APStaging entity endpoint | `/entity/APStaging/24.200.001/APStaging` |
| **Action Endpoint (Bill)** | REST path to the Bill creation endpoint | `/entity/APStaging/24.200.001/Bill` |

> **How to create an OAuth2 app in Acumatica:**
> 1. Go to **System > Security > Connected Applications** (SM201060).
> 2. Click **+** to add a new application.
> 3. Set **Client Grant Type** to `Password`.
> 4. Copy the generated **Client ID** and **Client Secret** into the Preferences screen.

### 2.2 Storecove Connection (if applicable)

| Field | What to enter | Example |
|-------|--------------|---------|
| **Storecove Base URL** | Base URL of the Storecove API | `https://api.storecove.com/api/v2` |
| **Storecove Token** | API bearer token provided by Storecove (stored encrypted) | `••••••••` |

### 2.3 Saving

Click **Save** (floppy disk icon) or press **Ctrl+S**.

---

## 3. Viewing Staged Invoices (List View)

Navigate to **Accounts Payable > AP Staging Form**. This opens the Generic Inquiry list (GI3010IL) showing all staged invoices.

### 3.1 Understanding the Columns

| Column | Description |
|--------|-------------|
| **Staging ID** | Auto-assigned unique number. Click to open the record. |
| **Doc. Type** | Invoice, Credit Adj, Debit Adj, or Prepayment. |
| **Vendor** | The Acumatica vendor code — may be blank if not yet resolved. |
| **Vendor Name** | The vendor name as received from the external system. |
| **Invoice Nbr.** | The vendor's own invoice number. |
| **Doc. Date** | Invoice date from the external system. |
| **Due Date** | Payment due date. |
| **Currency** | Currency code (e.g. MYR, USD). |
| **Description** | Invoice description. |
| **Status** | **On Hold** = under review; **Open** = ready; **Error** = problem occurred. |
| **Processing Status** | **Unprocessed** = AP Bill not yet created; **Processed** = AP Bill created. |
| **AP Doc. Type** | Populated after AP Bill is created — the type of the resulting AP document. |
| **AP Ref. Nbr.** | Populated after AP Bill is created — the AP Bill reference number you can look up in Payables. |
| **Branch** | The branch this invoice belongs to. |
| **Terms** | Payment terms (e.g. Net 30). |
| **Created** | When the staging record was first created. |

### 3.2 Using the Filters

The filter bar at the top allows you to narrow the list:

| Filter | How it works |
|--------|--------------|
| **Vendor Name** | Type part of the vendor name — shows all records where the name contains your text. |
| **Status** | Select from the dropdown — On Hold, Open, or Error. Leave blank to show all. |
| **Proc. Status** | Select Unprocessed or Processed. Leave blank to show all. |
| **Doc Date From** | Enter a date — shows only records with a Doc Date on or after this date. |
| **Doc Date To** | Enter a date — shows only records with a Doc Date on or before this date. |

Click the **refresh** button (circular arrow) after setting filters to apply them.

> **Tip:** To see only invoices that still need action, set **Proc. Status = Unprocessed**.

### 3.3 Opening a Record

Click anywhere on a row to open the full detail form (AP301090).

---

## 4. Reviewing a Staged Invoice

When you open a staging record, you will see the AP Staging Form (AP301090) with three areas: the header, a Details tab, and a Financial tab.

### 4.1 Header — Vendor Section

| Field | What to check / enter |
|-------|-----------------------|
| **Vendor** | If this is blank, the system could not automatically match the vendor name. Select the correct vendor from the selector. Once selected, Location, Terms, and Currency will auto-fill. |
| **Vendor Name** | The raw name received from the external system. You do not need to change this — it is for reference. |
| **Vendor Location** | Auto-filled when you select a Vendor. Change only if the invoice should go to a non-default location. |
| **Branch** | Select the company branch this invoice belongs to. |

### 4.2 Header — Dates Section

| Field | What to check / enter |
|-------|-----------------------|
| **Doc. Date** | The invoice date. Verify it matches the vendor's invoice. |
| **Due Date** | The payment due date. Auto-calculated from Terms if Terms is set; otherwise enter manually. |
| **Discount Date** | The last date to take an early-payment discount, if applicable. |
| **Pay Date** | The planned payment date. |
| **Fin. Period** | The financial period the invoice will be posted to. Defaults to the period of the Doc Date. |

### 4.3 Header — Document Section

| Field | What to check / enter |
|-------|-----------------------|
| **Doc Type** | Confirm the document type: Invoice, Credit Adj, Debit Adj, or Prepayment. |
| **Invoice Nbr.** | The vendor's invoice number. This is sent as the Vendor Ref in the created AP Bill. |
| **Terms** | Payment terms. Auto-filled from the Vendor; change if needed. |
| **Currency** | Auto-filled from the Vendor. Change only if the invoice is in a different currency. |
| **Status** | Change from **On Hold** to **Open** when the invoice is ready for processing. |
| **Processing Status** | Read-only. Set automatically by the system. |

### 4.4 Header — Description

A free-text description of the invoice. Edit if needed.

### 4.5 Header — Created AP Bill (read-only)

These fields are blank until you click **Create AP Bill**:

| Field | What it shows after processing |
|-------|-------------------------------|
| **AP Doc. Type** | The document type of the created AP Bill. |
| **AP Ref. Nbr.** | The reference number — click through to open the AP Bill in Payables. |

### 4.6 Details Tab — Line Items

Each row is a line on the invoice.

| Column | What to check / enter |
|--------|-----------------------|
| **Branch** | The branch for this line. Defaults to the header branch. |
| **Account** | The GL expense account to debit. Select from the chart of accounts. |
| **Subaccount** | The subaccount for the selected account, if applicable. |
| **Transaction Descr.** | Description of this line item. |
| **Qty** | Quantity. |
| **Unit Price** | Cost per unit. |
| **Discount Amount** | Discount amount for this line, if any. |
| **Amount** | Read-only. Automatically calculated as **(Qty × Unit Price) − Discount Amount**. |
| **Project** | Link to a project, if applicable. |

> **Adding a line:** Click the **+** button in the grid toolbar.
> **Removing a line:** Select the row and click the **−** (delete) button.

### 4.7 Financial Tab

These fields control the AP account posting and payment settings.

| Field | What to check / enter |
|-------|-----------------------|
| **AP Account** | The AP control account to credit when the bill is posted. Defaults from the vendor. |
| **AP Subaccount** | The subaccount for the AP account. |
| **Branch** | Branch for the financial posting. |
| **Pay Date** | Planned payment date (same as header Pay Date). |
| **Vendor Location** | Location used for the AP posting. |

### 4.8 Saving Changes

Click **Save** (floppy disk) or press **Ctrl+S** before proceeding to create the AP Bill.

---

## 5. Creating an AP Bill

Once you have reviewed and corrected the staging record:

1. Ensure the **Status** is set to **Open** (not On Hold).
2. Ensure the **Vendor** field is filled — the API call requires a valid vendor code.
3. Ensure at least one **Detail line** exists with an **Account** and an **Amount**.
4. Click the **Create AP Bill** button in the toolbar.

The system will:
1. Save the record.
2. Obtain an OAuth2 token from Acumatica using the credentials in Preferences.
3. Call the Acumatica REST API to create an AP Bill with all line items.
4. On success — set **Processing Status** to **Processed** and store the **AP Ref. Nbr.**
5. On failure — log an error in the trace log (see Troubleshooting).

> **The operation is asynchronous.** A progress spinner will appear in the toolbar. Wait for it to finish before navigating away. Do not click the button a second time.

### 5.1 Verifying the Created AP Bill

After the spinner disappears:
1. Refresh the record (press **Esc** then re-open, or click the refresh icon).
2. The **Created AP Bill** group will now show the **AP Doc. Type** and **AP Ref. Nbr.**
3. You can navigate directly to the AP Bill in **Accounts Payable > Bills and Adjustments** and search by that reference number.

---

## 6. Creating a Staging Record Manually

You can create staging records by hand if needed (without waiting for the webhook):

1. Navigate to **Accounts Payable > AP Staging Form** (the list view).
2. Click the **+** (New) button in the toolbar to open a blank AP Staging Form.
3. Fill in all required fields as described in Section 4.
4. Click **Save**.
5. Proceed to Section 5 to create the AP Bill when ready.

---

## 7. Field Reference

### Document Status Values

| Status | Meaning | When to use |
|--------|---------|-------------|
| **On Hold** | Invoice is under review, not ready for processing. | Default on arrival. Keep here while making corrections. |
| **Open** | Invoice has been reviewed and is ready. | Set this before clicking Create AP Bill. |
| **Error** | A processing error occurred. | Investigate the issue, correct the record, and retry. |

### Processing Status Values

| Processing Status | Meaning |
|-------------------|---------|
| **Unprocessed** | No AP Bill has been created yet. Action required. |
| **Processed** | AP Bill was successfully created. No further action needed. |

### Doc Type Values

| Doc Type | Acumatica AP Equivalent |
|----------|------------------------|
| Invoice | AP Bill (standard vendor invoice) |
| Credit Adj | AP Credit Adjustment |
| Debit Adj | AP Debit Adjustment |
| Prepayment | AP Prepayment |

---

## 8. Common Scenarios

### Scenario A — Invoice arrives but Vendor is blank

The external system sent a vendor name that does not exactly match any vendor in Acumatica.

**Steps:**
1. Open the staging record from the list.
2. In the **Vendor** field, click the magnifying glass and search for the correct vendor.
3. Select the vendor — Terms, Currency, and Location will auto-fill.
4. Save and proceed to Create AP Bill.

> **Prevention:** Ensure vendor names in your external system exactly match the **Account Name** field in Acumatica's vendor records (or contain it as a substring).

---

### Scenario B — Wrong account on a detail line

The external system sent an account code that mapped incorrectly.

**Steps:**
1. Open the staging record.
2. Click on the **Details** tab.
3. Click on the incorrect **Account** cell in the relevant row.
4. Select the correct GL account from the lookup.
5. Adjust the **Subaccount** if needed.
6. Save.

---

### Scenario C — Invoice has incorrect date

**Steps:**
1. Open the staging record.
2. Click on the **Doc. Date** field in the Dates group and enter the correct date.
3. Check **Due Date** and **Fin. Period** — they may need to be updated as well.
4. Save.

---

### Scenario D — Create AP Bill failed

If the Create AP Bill action fails:
1. Go to **System > Trace** (SM205070) and look for the most recent error entries from the APStaging module.
2. Common causes:
   - **Wrong Base URL** — check the URL in Preferences matches your Acumatica site.
   - **Invalid credentials** — verify the Client ID, Client Secret, Username and Password in Preferences.
   - **Endpoint path wrong** — the Entity/Action Endpoint in Preferences must match the version deployed in your Acumatica endpoint configuration.
   - **Vendor not set** — the Vendor field must be filled before the API call can succeed.
3. Correct the issue and click **Create AP Bill** again.

> Processing Status remains **Unprocessed** after a failure — it is safe to retry.

---

### Scenario E — Invoice was already processed (duplicate)

If you open a record and **Processing Status = Processed**, the AP Bill has already been created. Do not click Create AP Bill again — it will raise an error to prevent duplicates.

To find the existing AP Bill:
1. Note the **AP Ref. Nbr.** shown in the Created AP Bill section.
2. Go to **Accounts Payable > Bills and Adjustments** and search by that reference number.
