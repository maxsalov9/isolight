# IsoLight — быстрый порядок после установки Unity

## 1. Создать проект

Unity Hub → New project → Universal 3D → Project name: IsoLight

## 2. Скопировать файлы

Скопировать в корень Unity-проекта:

- `Docs/`
- `.gitignore`
- `.gitattributes`
- `README.md`
- `FIRST_CODEX_PROMPT_BATCH_1.md`

## 3. Git

```bash
cd /path/to/IsoLight
git init
git branch -M main
git lfs install
git add .
git commit -m "Initial IsoLight Unity project and docs"
```

## 4. GitHub

Создать private repo `isolight`, потом:

```bash
git remote add origin https://github.com/YOUR_USERNAME/isolight.git
git push -u origin main
```

## 5. Codex

Подключить GitHub repo к Codex и вставить содержимое:

`FIRST_CODEX_PROMPT_BATCH_1.md`
