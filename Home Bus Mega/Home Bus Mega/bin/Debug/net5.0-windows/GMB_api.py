import requests
import os
import sys

my_url = sys.argv[1]

output_file_path = os.path.join(os.path.dirname(__file__), 'GMB_api_result.txt')



def fetch_api_text(url):
    try:
        response = requests.get(url)
        response.raise_for_status()  # 检查请求是否成功
        return response.text  # 返回文本内容
    except requests.exceptions.RequestException as e:
        return f"Error: {e}"

api_text = fetch_api_text(my_url)
with open(output_file_path, 'w', encoding='utf-8') as file:
        file.write(api_text)  # 记录获取的文本

print(api_text)