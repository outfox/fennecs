import MarkdownIt from 'markdown-it';

interface HyperlinkPluginOptions {
  baseUrl: string; // Base URL for the hyperlink
}

export function hyperlinkPlugin(md: MarkdownIt, options?: Partial<HyperlinkPluginOptions>) {
  const opts: HyperlinkPluginOptions = {
    baseUrl: '/misc/Glossary.md#', // Can be customized through options
    ...options,
  };

  md.inline.ruler.after('emphasis', 'custom_hyperlink', (state, silent): boolean => {
    const start = state.pos;
    const max = state.posMax;
    const marker = '==';

    // Check if we have the opening ==
    if (state.src.substr(start, marker.length) !== marker) {
      return false;
    }

    // Look for the closing ==
    const end = state.src.indexOf(marker, start + marker.length);
    if (end === -1) {
      return false;
    }

    // Extract the text between the markers
    const text = state.src.substring(start + marker.length, end);

    if (!silent) {
      // Create the opening link token
      const openToken = state.push('link_open', 'a', 1);
      openToken.attrs = [['href', `${opts.baseUrl}${encodeURIComponent(text.trim().replace(/\s+/g, ' '))}`]];

      // Create the text token
      const textToken = state.push('text', '', 0);
      textToken.content = text;

      // Create the closing link token
      state.push('link_close', 'a', -1);
    }

    // Update state position to move past the closing marker
    state.pos = end + marker.length;
    return true;
  });
}
