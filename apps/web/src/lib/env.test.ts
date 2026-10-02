import { describe, expect, it } from "vitest";
import { parseEnv } from "./env";

describe("parseEnv", () => {
  it("accepts a complete environment", () => {
    expect(parseEnv({ API_URL: "http://localhost:5080" }).API_URL).toBe(
      "http://localhost:5080",
    );
  });

  it("names the missing variables", () => {
    expect(() => parseEnv({})).toThrow(/API_URL/);
  });
});
