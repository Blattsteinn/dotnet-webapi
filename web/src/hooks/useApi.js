import { useCallback, useEffect, useRef, useState } from 'react'

// Small data-fetching hook: runs `loader` on mount and whenever `deps` change,
// and exposes reload() for refreshes after a write. Results from a superseded
// run are dropped so a slow response cannot overwrite a newer one.
export function useApi(loader, deps = []) {
  const [data, setData] = useState(null)
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(true)
  const [nonce, setNonce] = useState(0)

  const loaderRef = useRef(loader)
  loaderRef.current = loader

  useEffect(() => {
    let active = true
    setLoading(true)
    setError(null)

    Promise.resolve()
      .then(() => loaderRef.current())
      .then((result) => {
        if (!active) return
        setData(result)
        setLoading(false)
      })
      .catch((cause) => {
        if (!active) return
        setError(cause)
        setData(null)
        setLoading(false)
      })

    return () => {
      active = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, nonce])

  const reload = useCallback(() => setNonce((value) => value + 1), [])

  return { data, error, loading, reload, setData }
}
