import type { z } from 'zod'

// Every API response is parsed at the boundary, so a contract drift fails loudly here
// rather than as `undefined` deep inside a component.
export async function getJson<T extends z.ZodType>(path: string, schema: T): Promise<z.infer<T>> {
  const response = await fetch(path)
  if (!response.ok) {
    throw new Error(`GET ${path} failed with ${response.status}`)
  }
  return schema.parse(await response.json())
}
