/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/types/embed.ts

export type TEmbedItem = {
  id: string;
  title: string;
  subTitle: string;
  icon: React.ReactNode;
  projectId: string;
  workspaceSlug: string;
};
