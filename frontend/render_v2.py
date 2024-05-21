
# %%

import sys
import json
import jinja2
import os
import hashlib
import minify_html
import urllib.parse


def render_simple_pages():
    pages = json.loads(open('pages.json', 'rb').read().decode('utf-8'))

    for page in pages:
        print(f"Rendering {page['src']}")
        ctx = {
            'pagedata': {},
            **GLOBAL_CTX,
            **(json.loads(open("JSON/"+page['json_file'], 'rb').read().decode('utf-8')) if 'json_file' in page else {}),
            **page,
            'navbar': get_navbar(pages, page['dest'])
        }

        if 'scripts' in ctx:
            ctx['scripts'] = map(filehash, ctx['scripts'])
        if 'inline_scripts' in ctx:
            ctx['inline_scripts'] = map(inlinescript, ctx['inline_scripts'])

        content = open(f"templates/{page['src']}", 'rb').read().decode('utf-8')
        j = jinja2.Environment(loader=jinja2.FileSystemLoader("templates/"))
        j.filters['encodeurl'] = urllib.parse.quote
        content = j.from_string(content).render(**ctx)
        dest = f"public/{page['dest']}"
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        with open(dest, 'wb') as f:
            f.write(minify(content).encode('utf-8'))


def filehash(s):
    with open(f'public{s}', 'rb') as f:
        h = hashlib.md5(f.read()).hexdigest()[0:8]
    return f"{s}?_={h}"


def inlinescript(s):
    with open(f'public{s}', 'rb') as f:
        return f.read().decode('utf-8')

def minify(s):
    if '--js' not in sys.argv: return s
    return minify_html.minify(s, minify_css=True, minify_js=True, do_not_minify_doctype=True,  keep_comments=False)

def get_navbar(pages, current_page):
    return [{'title': page['title'], 'link': page['dest'].removesuffix('.html'), 'active': page['dest'] == current_page, 'icon_svg': get_icon_svg(page['icon'])} for page in pages if 'title' in page and 'icon' in page]

def get_icon_svg(name):
    return open(f"icons/{name}.svg", 'rb').read().decode('utf-8')
# %%
GLOBAL_CTX = {}

render_simple_pages()
# %%
