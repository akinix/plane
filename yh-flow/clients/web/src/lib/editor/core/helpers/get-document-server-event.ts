/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/helpers/get-document-server-event.ts

import { DocumentCollaborativeEvents } from "../../core/constants/document-collaborative-events";
import type {
  TDocumentEventKey,
  TDocumentEventsClient,
  TDocumentEventsServer,
} from "../../core/types/document-collaborative-events";

export const getServerEventName = (clientEvent: TDocumentEventsClient): TDocumentEventsServer | undefined => {
  for (const key in DocumentCollaborativeEvents) {
    if (DocumentCollaborativeEvents[key as TDocumentEventKey].client === clientEvent) {
      return DocumentCollaborativeEvents[key as TDocumentEventKey].server;
    }
  }
  return undefined;
};
