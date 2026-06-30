// FLOW: DataTable — Sortable table with CSV export for analytics work items (per UI-SPEC)
"use client";

import React, { useState, useMemo } from "react";
import { Download, ArrowUpDown } from "lucide-react";
import { Button } from "@/lib/ui/button";
import { download, generateCsv, mkConfig } from "export-to-csv";
import type { TChartDatum } from "@plane/types";

interface Column {
  key: string;
  label: string;
  sortable?: boolean;
}

interface DataTableProps {
  data: TChartDatum[];
  columns: Column[];
  total?: number;
}

export const DataTable: React.FC<DataTableProps> = ({ data, columns, total }) => {
  const [sortKey, setSortKey] = useState<string>("count");
  const [sortDir, setSortDir] = useState<"asc" | "desc">("desc");

  const handleSort = (key: string) => {
    if (sortKey === key) {
      setSortDir((prev) => (prev === "asc" ? "desc" : "asc"));
    } else {
      setSortKey(key);
      setSortDir("desc");
    }
  };

  const sortedData = useMemo(() => {
    return [...data].toSorted((a, b) => {
      const aVal = a[sortKey as keyof TChartDatum] ?? 0;
      const bVal = b[sortKey as keyof TChartDatum] ?? 0;
      if (typeof aVal === "number" && typeof bVal === "number") {
        return sortDir === "desc" ? bVal - aVal : aVal - bVal;
      }
      return 0;
    });
  }, [data, sortKey, sortDir]);

  const totalCount = total ?? data.reduce((sum, d) => sum + (typeof d.count === "number" ? d.count : 0), 0);

  const handleExportCSV = () => {
    const csvConfig = mkConfig({
      fieldSeparator: ",",
      filename: `analytics-export-${Date.now()}`,
      decimalSeparator: ".",
      useKeysAsHeaders: true,
      showTitle: false,
    });

    const csvData = sortedData.map((d) => ({
      类别: d.name,
      数量: d.count,
      占比: `${totalCount > 0 ? Math.round(((typeof d.count === "number" ? d.count : 0) / totalCount) * 100) : 0}%`,
    }));

    const csv = generateCsv(csvConfig)(csvData);
    download(csvConfig)(csv);
  };

  return (
    <div className="border-custom-border-200 overflow-hidden rounded-md border">
      {/* Table header with export button */}
      <div className="border-custom-border-200 bg-custom-background-90 flex items-center justify-between border-b px-4 py-2">
        <span className="text-custom-text-300 text-13 font-semibold">数据详情</span>
        <Button variant="neutral-primary" size="sm" onClick={handleExportCSV} title="导出 CSV">
          <Download className="size-3.5" />
          <span>导出 CSV</span>
        </Button>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="text-sm w-full text-left">
          <thead>
            <tr className="border-custom-border-200 border-b">
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={`text-custom-text-300 px-4 py-2.5 text-13 font-semibold ${
                    col.sortable ? "hover:text-custom-text-200 cursor-pointer" : ""
                  }`}
                  onClick={() => col.sortable && handleSort(col.key)}
                >
                  <div className="flex items-center gap-1">
                    {col.label}
                    {col.sortable && <ArrowUpDown className="text-custom-text-400 size-3" />}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {sortedData.map((row) => (
              <tr
                key={row.key}
                className="border-custom-border-200 hover:bg-custom-background-80 border-b last:border-b-0"
              >
                {columns.map((col) => {
                  const value = row[col.key as keyof TChartDatum];
                  return (
                    <td key={col.key} className="text-custom-text-200 px-4 py-2.5">
                      {col.key === "percentage"
                        ? `${totalCount > 0 ? Math.round(((typeof row.count === "number" ? row.count : 0) / totalCount) * 100) : 0}%`
                        : col.key === "count"
                          ? (value ?? "-")
                          : (value ?? "-")}
                    </td>
                  );
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
