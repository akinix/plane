// FLOW: Type declaration for prosemirror-codemark (code mark extension)
declare module "prosemirror-codemark" {
  import type { MarkSpec, Node as ProseMirrorNode, Schema } from "prosemirror-model";
  import type { Command, Plugin } from "prosemirror-state";
  
  interface CodemarkOptions {
    markType: import("prosemirror-model").MarkType;
  }
  
  function codemark(options: CodemarkOptions): Plugin[];
  
  export default codemark;
}
