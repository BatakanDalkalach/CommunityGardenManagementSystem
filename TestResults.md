# Test Results Summary

**Date:** 2026-04-06  
**Branch:** main

## Build Status

| Project | Result | Warnings | Errors |
|---------|--------|----------|--------|
| WebApplication1 | **PASSED** | 0 | 0 |
| WebApplication1.Tests | **PASSED** | 0 | 0 |

## Test Run

| Metric | Value |
|--------|-------|
| Total Tests | **106** |
| Passed | **106** |
| Failed | 0 |
| Skipped | 0 |
| Duration | ~2.3 s |

## Test Suites

| Suite | Tests | Status |
|-------|-------|--------|
| `HarvestRecordTests` | 21 | All Passed |
| `AnnouncementTests` | 16 | All Passed |
| `MaintenanceRequestTests` | 20 | All Passed |
| `MemberManagementServiceTests` | 13 | All Passed |
| `PlotManagementServiceTests` | 15 | All Passed |
| **Total** | **106** | **All Passed** |

## New Tests Added (HarvestRecord Search/Filter)

The following 7 tests were added to cover the new organic-only filter and crop name search functionality:

| Test | Description |
|------|-------------|
| `FilterByOrganic_ReturnsOnlyOrganicRecords` | Verifies `.Where(IsOrganicCertified)` returns only certified records |
| `FilterByOrganic_ExcludesNonOrganicRecords` | Confirms no non-organic records appear in organic filter |
| `FilterByOrganic_ReturnsSeededOrganicRecord` | Asserts the seeded "Cherry Tomatoes" is the sole organic record |
| `FilterByOrganic_AfterAddingOrganicRecord_IncludesNewRecord` | Dynamic test — new organic record appears after save |
| `FilterByCropName_PartialMatch_ReturnsMatches` | `Contains("Tomato")` matches "Cherry Tomatoes" |
| `FilterByCropName_WithNonMatchingTerm_ReturnsEmpty` | Non-existent crop name returns empty list |
| `FilterByCropNameAndOrganic_Combined_ReturnsCorrectSubset` | Combined crop + organic filter excludes non-matching rows |

## Feature Added: HarvestRecords Search & Filter

### Controller (`HarvestRecordsController.cs`)
- Added `bool organicOnly = false` parameter to `Index` action
- Applied `Where(h => h.IsOrganicCertified)` when `organicOnly` is true
- Exposed `ViewBag.OrganicOnly` for the view

### View (`Views/HarvestRecords/Index.cshtml`)
- Added **"Organic only"** checkbox to the filter bar
- "Clear" button now appears when either the crop filter or organic filter is active
- Pagination links preserve `organicOnly` state across pages
