// FLOW: Adapted from Plane @/constants/calendar.ts
// Simplified for yh-flow: Chinese day labels, English month labels

export const MONTHS_LIST: Record<number, { shortTitle: string; title: string }> = {
  1: { shortTitle: "1月", title: "一月" },
  2: { shortTitle: "2月", title: "二月" },
  3: { shortTitle: "3月", title: "三月" },
  4: { shortTitle: "4月", title: "四月" },
  5: { shortTitle: "5月", title: "五月" },
  6: { shortTitle: "6月", title: "六月" },
  7: { shortTitle: "7月", title: "七月" },
  8: { shortTitle: "8月", title: "八月" },
  9: { shortTitle: "9月", title: "九月" },
  10: { shortTitle: "10月", title: "十月" },
  11: { shortTitle: "11月", title: "十一月" },
  12: { shortTitle: "12月", title: "十二月" },
};

export const DAY_SHORT_LABELS = ["日", "一", "二", "三", "四", "五", "六"];

export const DAY_SHORT_LABELS_EN = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
