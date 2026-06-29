/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/ce/constants/assets.ts

// helpers
import type { TAssetMetaDataRecord } from "../../core/helpers/assets";
// local imports
import type { ADDITIONAL_EXTENSIONS } from "./extensions";

export const ADDITIONAL_ASSETS_META_DATA_RECORD: Partial<Record<ADDITIONAL_EXTENSIONS, TAssetMetaDataRecord>> = {};
