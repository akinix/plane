/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/extensions/enter-key.ts

import { Extension } from "@tiptap/core";
// constants
import { CORE_EXTENSIONS } from "../../core/constants/extension";

export const EnterKeyExtension = (onEnterKeyPress?: () => void) =>
  Extension.create({
    name: CORE_EXTENSIONS.ENTER_KEY,

    addKeyboardShortcuts(this) {
      return {
        Enter: () => {
          const { activeDropbarExtensions } = this.editor.storage.utility;

          if (activeDropbarExtensions.length === 0) {
            onEnterKeyPress?.();
            return true;
          }

          return false;
        },
        "Shift-Enter": ({ editor }) =>
          editor.commands.first(({ commands }) => [
            () => commands.newlineInCode(),
            () => commands.splitListItem(CORE_EXTENSIONS.LIST_ITEM),
            () => commands.splitListItem(CORE_EXTENSIONS.TASK_ITEM),
            () => commands.createParagraphNear(),
            () => commands.liftEmptyBlock(),
            () => commands.splitBlock(),
          ]),
      };
    },
  });
