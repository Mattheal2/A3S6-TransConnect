import http.server
import os
import requests

class StaticFileHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        # Get the requested file path
        path = self.path.split("?")[0].removesuffix("/")
        
        path = f"public{path}"

        # Look for the requested file, or default to index.html if it's a directory
        if os.path.exists(path) and not os.path.isdir(path):
            # Serve the file
            self.path = "/" + path
            return super().do_GET()
        elif os.path.exists(path + ".html"):
            # Serve the .html version of the file
            self.path = "/" + path + ".html"
            return super().do_GET()
        elif os.path.exists(path + "/index.html"):
            # Serve the index.html inside the directory
            self.path = "/" + path + "/index.html"
            return super().do_GET()
        else:
            # File not found
            self.send_error(404, "File not found")

    def do_API(self):
        api_url = 'http://127.0.0.1:5132' + self.path
        print(f"Requesting {api_url}")
        if 'Content-Length' in self.headers:
            body = self.rfile.read(int(self.headers['Content-Length'])).decode('utf-8')
        else:
            body = None
        
        headers_filtered = {k: v for k, v in self.headers.items() if k.lower() not in ['host', 'connection', 'accept-encoding']}
        response = requests.request(self.command, api_url, headers=headers_filtered, data=body, allow_redirects=False)
        self.send_response(response.status_code)
        for header, value in response.headers.items():
            if header.lower() in ['transfer-encoding', 'connection', 'content-encoding', 'strict-transport-security']: continue
            if header.lower() == 'set-cookie':
                for v in value.split(', '):
                    self.send_header(header, v)
            else:
                self.send_header(header, value)
        self.end_headers()
        self.wfile.write(response.content)
        self.wfile.flush()


def register_watchdog():
    # on any changes on the templates folder, re-render the public folder
    import watchdog.events
    import watchdog.observers
    import render_v2

    class Handler(watchdog.events.PatternMatchingEventHandler):
        def on_any_event(self, event):
            print(f"Detected change in {event.src_path}")
            try:
                render_v2.render_simple_pages()
            except Exception as e:
                import traceback
                traceback.print_exc()
        
    observer = watchdog.observers.Observer()
    observer.schedule(Handler(), "templates/", recursive=True)
    observer.start()

    
if __name__ == '__main__':
    register_watchdog()
    class CustomHandler(StaticFileHandler):
        def do_GET(self):
            if self.path.startswith('/api/') or self.path.startswith('/swagger'):
                return self.do_API()
            else:
                return super().do_GET()
        def do_POST(self):
            return self.do_API()
        def do_DELETE(self):
            return self.do_API()
        

    http.server.test(HandlerClass=CustomHandler)