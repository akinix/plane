/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/ce/types/issue-embed.ts

export type TEmbedConfig = {
  issue?: TIssueEmbedConfig;
};

export type TReadOnlyEmbedConfig = TEmbedConfig;

export type TIssueEmbedConfig = {
  widgetCallback: ({
    issueId,
    projectId,
    workspaceSlug,
  }: {
    issueId: string;
    projectId: string | undefined;
    workspaceSlug: string | undefined;
  }) => React.ReactNode;
};
