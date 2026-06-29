// FLOW: Type stub for Plane's use-multiple-select hook
// FLOW: Phase 17 is read-only — no selection interaction needed yet
// FLOW: Provides TSelectionHelper type for gantt-view sidebar type compatibility
/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 *
 * Stub — original is at apps/web/core/hooks/use-multiple-select.ts
 */

export type TSelectionHelper = {
  handleClearSelection: () => void;
  handleEntityClick: (event: React.MouseEvent, entityID: string, groupId: string) => void;
  getIsEntitySelected: (entityID: string) => boolean;
  getIsEntityActive: (entityID: string) => boolean;
  handleGroupClick: (groupID: string) => void;
  isGroupSelected: (groupID: string) => "empty" | "partial" | "complete";
  isSelectionDisabled: boolean;
};
