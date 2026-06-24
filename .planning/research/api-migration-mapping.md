# API Migration Mapping: Django → .NET

**Research Date:** 2026-06-16

## Authentication Endpoints

| Plane Django Route            | .NET Endpoint                      | Module   |
| ----------------------------- | ---------------------------------- | -------- |
| `POST /auth/sign-in/`         | `POST /auth/sign-in`               | Identity |
| `POST /auth/sign-up/`         | `POST /auth/sign-up`               | Identity |
| `POST /auth/sign-out/`        | `POST /auth/sign-out`              | Identity |
| `POST /auth/forgot-password/` | `POST /auth/forgot-password`       | Identity |
| `POST /auth/reset-password/`  | `POST /auth/reset-password`        | Identity |
| `POST /auth/magic-sign-in/`   | `POST /auth/magic-sign-in`         | Identity |
| `POST /auth/magic-sign-up/`   | `POST /auth/magic-sign-up`         | Identity |
| `GET /auth/oauth/github/`     | `GET /auth/oauth/github`           | Identity |
| `GET /auth/oauth/gitlab/`     | `GET /auth/oauth/gitlab`           | Identity |
| `GET /auth/oauth/gitea/`      | `GET /auth/oauth/gitea`            | Identity |
| `GET /auth/oauth/google/`     | `GET /auth/oauth/google`           | Identity |
| `POST /auth/github/callback/` | `POST /auth/oauth/github/callback` | Identity |
| `GET /auth/me/`               | `GET /auth/me`                     | Identity |
| `POST /auth/api-tokens/`      | `POST /auth/api-tokens`            | Identity |

## Workspace Endpoints

| Plane Django Route                            | HTTP Method | .NET Endpoint          | Module    |
| --------------------------------------------- | ----------- | ---------------------- | --------- |
| `/api/v1/workspaces/`                         | GET         | List user's workspaces | Workspace |
| `/api/v1/workspaces/`                         | POST        | Create workspace       | Workspace |
| `/api/v1/workspaces/{slug}/`                  | GET         | Get workspace details  | Workspace |
| `/api/v1/workspaces/{slug}/`                  | PUT/PATCH   | Update workspace       | Workspace |
| `/api/v1/workspaces/{slug}/`                  | DELETE      | Delete workspace       | Workspace |
| `/api/v1/workspaces/{slug}/members/`          | GET         | List members           | Workspace |
| `/api/v1/workspaces/{slug}/members/`          | POST        | Invite member          | Workspace |
| `/api/v1/workspaces/{slug}/members/{id}/`     | PUT/PATCH   | Update member role     | Workspace |
| `/api/v1/workspaces/{slug}/members/{id}/`     | DELETE      | Remove member          | Workspace |
| `/api/v1/workspaces/{slug}/invitations/`      | GET         | List invitations       | Workspace |
| `/api/v1/workspaces/{slug}/invitations/`      | POST        | Create invitation      | Workspace |
| `/api/v1/workspaces/{slug}/invitations/{id}/` | DELETE      | Revoke invitation      | Workspace |

## Project Endpoints

| Plane Django Route                                              | .NET Endpoint | Module  |
| --------------------------------------------------------------- | ------------- | ------- |
| `GET /api/v1/workspaces/{slug}/projects/`                       | Same          | Project |
| `POST /api/v1/workspaces/{slug}/projects/`                      | Same          | Project |
| `GET /api/v1/workspaces/{slug}/projects/{id}/`                  | Same          | Project |
| `PUT /api/v1/workspaces/{slug}/projects/{id}/`                  | Same          | Project |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/`               | Same          | Project |
| `GET /api/v1/workspaces/{slug}/projects/{id}/members/`          | Same          | Project |
| `POST /api/v1/workspaces/{slug}/projects/{id}/members/`         | Same          | Project |
| `GET /api/v1/workspaces/{slug}/projects/{id}/members/{mid}/`    | Same          | Project |
| `PUT /api/v1/workspaces/{slug}/projects/{id}/members/{mid}/`    | Same          | Project |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/members/{mid}/` | Same          | Project |

## Issue (WorkItem) Endpoints

| Plane Django Route                                                      | .NET Endpoint | Module    |
| ----------------------------------------------------------------------- | ------------- | --------- |
| `GET /api/v1/workspaces/{slug}/projects/{id}/issues/`                   | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/projects/{id}/issues/`                  | Same          | WorkItems |
| `GET /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/`             | Same          | WorkItems |
| `PUT /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/`             | Same          | WorkItems |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/`          | Same          | WorkItems |
| `GET /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/comments/`    | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/comments/`   | Same          | WorkItems |
| `GET /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/activity/`    | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/sub-issues/` | Same          | WorkItems |
| `GET /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/links/`       | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/projects/{id}/issues/{iid}/links/`      | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/projects/{id}/issues/bulk/`             | Same          | WorkItems |

## Cycle Endpoints

| Plane Django Route                                                           | .NET Endpoint | Module |
| ---------------------------------------------------------------------------- | ------------- | ------ |
| `GET /api/v1/workspaces/{slug}/projects/{id}/cycles/`                        | Same          | Cycle  |
| `POST /api/v1/workspaces/{slug}/projects/{id}/cycles/`                       | Same          | Cycle  |
| `GET /api/v1/workspaces/{slug}/projects/{id}/cycles/{cid}/`                  | Same          | Cycle  |
| `PUT /api/v1/workspaces/{slug}/projects/{id}/cycles/{cid}/`                  | Same          | Cycle  |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/cycles/{cid}/`               | Same          | Cycle  |
| `POST /api/v1/workspaces/{slug}/projects/{id}/cycles/{cid}/issues/`          | Same          | Cycle  |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/cycles/{cid}/issues/`        | Same          | Cycle  |
| `POST /api/v1/workspaces/{slug}/projects/{id}/cycles/{cid}/transfer-issues/` | Same          | Cycle  |

## Module Endpoints

| Plane Django Route                                                   | .NET Endpoint | Module |
| -------------------------------------------------------------------- | ------------- | ------ |
| `GET /api/v1/workspaces/{slug}/projects/{id}/modules/`               | Same          | Module |
| `POST /api/v1/workspaces/{slug}/projects/{id}/modules/`              | Same          | Module |
| `GET /api/v1/workspaces/{slug}/projects/{id}/modules/{mid}/`         | Same          | Module |
| `PUT /api/v1/workspaces/{slug}/projects/{id}/modules/{mid}/`         | Same          | Module |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/modules/{mid}/`      | Same          | Module |
| `POST /api/v1/workspaces/{slug}/projects/{id}/modules/{mid}/issues/` | Same          | Module |

## Page Endpoints

| Plane Django Route                                            | .NET Endpoint | Module |
| ------------------------------------------------------------- | ------------- | ------ |
| `GET /api/v1/workspaces/{slug}/projects/{id}/pages/`          | Same          | Page   |
| `POST /api/v1/workspaces/{slug}/projects/{id}/pages/`         | Same          | Page   |
| `GET /api/v1/workspaces/{slug}/projects/{id}/pages/{pid}/`    | Same          | Page   |
| `PUT /api/v1/workspaces/{slug}/projects/{id}/pages/{pid}/`    | Same          | Page   |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/pages/{pid}/` | Same          | Page   |
| `GET /api/v1/workspaces/{slug}/pages/`                        | Same          | Page   |

## State, Label, View, Estimate Endpoints

| Plane Django Route                                         | .NET Endpoint | Module    |
| ---------------------------------------------------------- | ------------- | --------- |
| `GET/POST /api/v1/workspaces/{slug}/projects/{id}/states/` | Same          | WorkItems |
| `GET/PUT/DELETE .../states/{sid}/`                         | Same          | WorkItems |
| `GET/POST .../labels/`                                     | Same          | WorkItems |
| `GET/PUT/DELETE .../labels/{lid}/`                         | Same          | WorkItems |
| `GET/POST .../views/`                                      | Same          | View      |
| `GET/PUT/DELETE .../views/{vid}/`                          | Same          | View      |
| `GET/POST .../estimates/`                                  | Same          | WorkItems |
| `GET/PUT/DELETE .../estimates/{eid}/`                      | Same          | WorkItems |

## Webhook & Integration Endpoints

| Plane Django Route                                                     | .NET Endpoint | Module      |
| ---------------------------------------------------------------------- | ------------- | ----------- |
| `GET/POST /api/v1/workspaces/{slug}/webhooks/`                         | Same          | Webhooks    |
| `GET/PUT/DELETE .../webhooks/{wid}/`                                   | Same          | Webhooks    |
| `GET .../webhooks/{wid}/logs/`                                         | Same          | Webhooks    |
| `POST /api/v1/workspaces/{slug}/projects/{id}/github-repository-sync/` | Same          | Integration |
| `GET /api/v1/workspaces/{slug}/projects/{id}/github-repositories/`     | Same          | Integration |
| `GET /api/v1/workspaces/{slug}/projects/{id}/slack-project-sync/`      | Same          | Integration |
| `POST /api/v1/workspaces/{slug}/projects/{id}/slack-project-sync/`     | Same          | Integration |

## Import/Export Endpoints

| Plane Django Route                       | .NET Endpoint | Module    |
| ---------------------------------------- | ------------- | --------- |
| `GET /api/v1/workspaces/{slug}/export/`  | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/export/` | Same          | WorkItems |
| `GET /api/v1/workspaces/{slug}/import/`  | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/import/` | Same          | WorkItems |

## Analytics Endpoints

| Plane Django Route                              | .NET Endpoint | Module    |
| ----------------------------------------------- | ------------- | --------- |
| `GET /api/v1/workspaces/{slug}/analytics/`      | Same          | Analytics |
| `GET /api/v1/workspaces/{slug}/analytics/{id}/` | Same          | Analytics |
| `GET /api/v1/workspaces/{slug}/dashboard/`      | Same          | Analytics |

## Public Space Endpoints

| Plane Django Route                                        | .NET Endpoint | Module    |
| --------------------------------------------------------- | ------------- | --------- |
| `GET /api/public/workspaces/{slug}/`                      | Same          | Workspace |
| `GET /api/public/workspaces/{slug}/projects/{id}/issues/` | Same          | WorkItems |

## Intake/Inbox Endpoints

| Plane Django Route                                             | .NET Endpoint | Module    |
| -------------------------------------------------------------- | ------------- | --------- |
| `GET /api/v1/workspaces/{slug}/projects/{id}/intake/`          | Same          | WorkItems |
| `POST /api/v1/workspaces/{slug}/projects/{id}/intake/`         | Same          | WorkItems |
| `DELETE /api/v1/workspaces/{slug}/projects/{id}/intake/{iid}/` | Same          | WorkItems |
