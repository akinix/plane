/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/types/slash-commands-suggestion.ts

import type { Editor, Range } from "@tiptap/core";
import type { CSSProperties } from "react";
import type { TEditorCommands } from "../../core/types";

export type CommandProps = {
  editor: Editor;
  range: Range;
};

export type TSlashCommandSectionKeys = "general" | "text-colors" | "background-colors";

export type ISlashCommandItem = {
  commandKey: TEditorCommands;
  key: string;
  title: string;
  description: string;
  searchTerms: string[];
  icon: React.ReactNode;
  iconContainerStyle?: CSSProperties;
  command: ({ editor, range }: CommandProps) => void;
  badge?: React.ReactNode;
};
