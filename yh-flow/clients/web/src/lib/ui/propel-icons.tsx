// FLOW: Adapter for @plane/propel/icons → lucide-react
// Plane's UI components import icons with "Icon" suffix from @plane/propel/icons.
// This file re-exports lucide-react icons with the same names so forked UI code compiles.

import {
  Link as LinkIcon,
  Copy as CopyIcon,
  Globe as GlobeIcon,
  Lock as LockIcon,
  ExternalLink as NewTabIcon,
  Check as CheckIcon,
  Search as SearchIcon,
  ChevronDown as ChevronDownIcon,
  ChevronLeft as ChevronLeftIcon,
  ChevronRight as ChevronRightIcon,
  ChevronUp as ChevronUpIcon,
  ChevronDown,
  MoreHorizontal,
  Info,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

// FLOW: replaces @plane/propel/icons DropdownIcon
export function DropdownIcon({ className = "text-current", ...rest }: { className?: string; [key: string]: any }) {
  return <ChevronDown className={className} {...rest} />;
}

// FLOW: replaces @plane/propel/icons ISvgIcons type
export type ISvgIcons = { className?: string; color?: string; height?: string; width?: string; [key: string]: any };

export type { LucideIcon };
export {
  LinkIcon,
  CopyIcon,
  GlobeIcon,
  LockIcon,
  NewTabIcon,
  CheckIcon,
  SearchIcon,
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronUpIcon,
  MoreHorizontal,
  Info,
};
