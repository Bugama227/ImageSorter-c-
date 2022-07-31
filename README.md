#en
Program for comparing images for duplicates using simple descaling and hashing algoritm. it's not quite optimized and highly demanding on cpu capabilities. 
it has linear coreliation to amount of cpu threads. On a parallel universe where i would have any interest in C# i would like to rewrite it from a scratch. 
or atleast give some look on parallel calculations.

Also has interface for sorting images into different folders using hotkeys.
All added paths might be saved into profile. Which can be loaded to prevent reentering paths.

#ru
Программа для сравнивания изображений и поиска дубликатов. 
В процессе обработки массива изображений они уменьшаются и превращаются в чёрнобелые точки преобразовываемые к бинарной строке.
Алгориртм полностью рабочий и имеет точность близкую к аналогам, написанным людьми, которые умеют писать код.
Проблема же заключается в линейной зависимости от вычислительной мощности системы. Даже со всеми(довольно кривыми) попытками в параллелизацию программа сильно зависима от количества ядер.

Основой приложения является интерфейс для рассортировки изображений по папкам. Папки задаются вручную и к пути привязывается горячая клавиша. 
Однако, сортировка сама по себе является ручной.
Также была добавлена система сохранения профилей работы интерфейса.
