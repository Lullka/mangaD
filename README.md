# MangaSharp

### Inspiração em Dmanga de [dkeas](https://github.com/dkeas).
### Muitas das opções do criador estão neste script/programa.


MangaSharp é um programa/script de Windows feito em C# com o intuito de fazer download de mangas do site Mangahosted.com (Site brasileiro de mangas). Não há um número máximo para mangas ou capitulos. Diga o manga e seus capítulos e sente-se em sua cadeira para ler. Seu métado de pesquisa é Web Scraping.


# Utilização

OBS: O local padrão de download é `C:\Users\userLogado\Pictures\mangaD`.

Pesquisa o nome de seu manga:

`mangas <NomeDoSeuManga>`

Não é possivel fazer a pesquisa em japonês.

`キメツ学園!`

Ele mostrará todos os capítulos e irá perguntar "Quais capítulos deseja instalar?", opções de selecionar o(s) capítulo(s):

1. Instalar todos os capítulos:

  `todos`
  
Abaixará todos os capítulos.

2. Instalação com intervalo:

  `Inicio-fim`

Ex:

  `11-17`

Irá instalar do 11 ao 17.

3. Selecionar capítulos específicos

  `numeroDoCap,numeroDoCap,numeroDoCap,numeroDoCap,...`

Ex:

  `10,20,30,40`

Irá instalar apenas os capítulos 10, 20, 30 e 40.

OBS: As vírgulas com campos vazios não serão instaladas 


## License

The script is available as open source under the terms of the [MIT License](http://opensource.org/licenses/MIT).
