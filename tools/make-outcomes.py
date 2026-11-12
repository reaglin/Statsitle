"""Writes docs/OUTCOMES.md: every module's outcomes with their stable IDs, the items that teach each one, the graded
exercises and the check and practice questions that give evidence for it (plan 13.6, from PLC+'s tools/make-outcomes.py).

    python tools/make-outcomes.py

Run it after changing a module's outcomes, its items' objectives, its exercises or its questions. The test
ContentTests.Content_every_outcome_is_taught_by_an_item_and_asked_in_its_module_check holds the rule the page shows:
no outcome without an item that teaches it and a check question that assesses it.
"""
import json
import os
import re

REPO = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..')
CONTENT = os.path.join(REPO, 'content', 'egn3443')


def front(path):
    text = open(path, encoding='utf-8').read().replace('\r\n', '\n')
    m = re.match(r'---\n(.*?)\n---\n', text, re.S)
    meta, outcomes, key = {}, [], None
    for line in (m.group(1) if m else '').splitlines():
        if re.match(r'^\s+- ', line):
            if key == 'outcomes':
                oid, words = line.strip()[2:].split(':', 1)
                outcomes.append((oid.strip(), words.strip()))
            continue
        k, _, v = line.partition(':')
        key = k.strip()
        v = v.strip()
        meta[key] = [x.strip() for x in v[1:-1].split(',') if x.strip()] if v.startswith('[') else v
    meta['outcomes'] = outcomes
    return meta


def is_exercise(task_id):
    """A graded exercise is a task whose JSON asks for results (an Example only shows them)."""
    for name in os.listdir(os.path.join(CONTENT, 'tasks')):
        task = json.load(open(os.path.join(CONTENT, 'tasks', name), encoding='utf-8'))
        if task.get('id') == task_id:
            return bool(task.get('asked') or task.get('ask'))
    return False


manifest = json.load(open(os.path.join(CONTENT, 'manifest.json'), encoding='utf-8'))
lines = ['# Statistics+ — outcomes', '',
         'Every module\'s outcomes with their **stable IDs** (progress, mastery and reports key off them), in the course\'s own MO',
         'wording from https://reaglin.github.io/EGN3443/; the items that teach each one; the exercises graded by the engine; and',
         'the module-check and practice questions that give evidence for it. **Ron reviews them** (plan 13.6).',
         'Written by `tools/make-outcomes.py`; do not edit by hand. `ContentTests` fails if an outcome has no item or no check question.', '']
totals = [0, 0, 0, 0]
for folder in manifest['modules']:
    mdir = os.path.join(CONTENT, folder)
    mod = front(os.path.join(mdir, 'module.md'))
    items = [front(os.path.join(mdir, f)) for f in sorted(os.listdir(mdir)) if f.endswith('.md') and f != 'module.md']
    check_q, practice_q, exercises = {}, {}, {}
    for it in items:
        if it.get('quiz'):
            quiz = json.load(open(os.path.join(CONTENT, it['quiz']), encoding='utf-8'))
            bucket = check_q if it.get('kind') == 'assess' else practice_q
            for q in quiz['questions']:
                if q.get('objective'):
                    bucket[q['objective']] = bucket.get(q['objective'], 0) + 1
        if it.get('exercise') and is_exercise(it['exercise']):
            for o in it.get('objectives') or []:
                exercises[o] = exercises.get(o, 0) + 1
    number = mod.get('number')
    lines += [f'## Module {number}: {mod.get("title")}', '']
    if not mod['outcomes']:
        lines += ['No outcomes yet.', '']
        continue
    lines += ['| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |', '|---|---|---|---|---|']
    for oid, words in mod['outcomes']:
        taught = [f'{number}.{n} {it.get("title")}' for n, it in enumerate(items, 1)
                  if oid in (it.get('objectives') or []) and it.get('kind') != 'assess']
        c, p, e = check_q.get(oid, 0), practice_q.get(oid, 0), exercises.get(oid, 0)
        totals[0] += 1
        totals[1] += e
        totals[2] += c
        totals[3] += p
        lines.append(f'| `{oid}` | {words.replace("|", "/")} | {"; ".join(taught) or "**none**"} | {e or "—"} | {c} / {p} |')
    lines.append('')
lines += ['## Totals', '',
          f'{totals[0]} outcomes; {totals[1]} graded exercises, {totals[2]} module-check questions and {totals[3]} practice questions',
          'list one of them (an exercise counts once for each outcome its item lists).', '']
open(os.path.join(REPO, 'docs', 'OUTCOMES.md'), 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
print('wrote docs/OUTCOMES.md')
