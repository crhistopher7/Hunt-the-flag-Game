# -*- coding: utf-8 -*-
"""
@author: juliano soares
  
"""


def cortaCentro(filename, name, dir_in, dir_out):
    from PIL import Image

    src_map = dir_in + filename
    img = Image.open(src_map)
    w, h = img.size
    h = h / 4
    w = w / 4

    inix = 0
    iniy = 0
    fimx = 0
    fimy = 0
    for i in range(4):
        fimy += h
        for j in range(4):
            fimx += w
            box = (inix, iniy, fimx, fimy,)
            cropped_image = img.crop(box)
            cropped_image.save(dir_out + name + '_' +
                               str(i) + '_' + str(j) + '.png')
            inix += w
        inix = 0
        fimx = 0
        iniy += h


def main():
    mapa_input = 'MapasOriginais/'
    filename = 'Streets.png'
    name = 'Streets'
    mapa_output = 'MapasCortados/Streets/'

    cortaCentro(filename, name, mapa_input, mapa_output)


if __name__ == '__main__':
    main()
